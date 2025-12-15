using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PortSafe.Services
{
    public class GmailService
    {
        private readonly string _email;
        private readonly string _appPassword;

        public GmailService(string email, string appPassword)
        {
            _email = email;
            _appPassword = appPassword;
        }

        public async Task EnviarAsync(string para, string assunto, string corpoHtml)
        {
            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("PortSafe", _email));
            mensagem.To.Add(new MailboxAddress("", para));
            mensagem.Subject = assunto;
            
            // Criar apenas corpo de texto simples (sem HTML por enquanto)
            var corpoTexto = StripHtml(corpoHtml);
            mensagem.Body = new TextPart("plain") { Text = corpoTexto };

            using var cliente = new SmtpClient();
            
            try
            {
                // Configurações de timeout
                cliente.Timeout = 120000; // 120 segundos
                
                // Desabilita verificação de certificado SSL
                cliente.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                Console.WriteLine($"[Email] Tentando enviar email de texto simples...");
                Console.WriteLine($"[Email] De: {_email}");
                Console.WriteLine($"[Email] Para: {para}");
                Console.WriteLine($"[Email] Assunto: {assunto}");
                Console.WriteLine($"[Email] Conectando ao servidor SMTP (porta 465 SSL)...");
                
                // Tenta primeiro com SSL direto na porta 465
                try
                {
                    await cliente.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                }
                catch
                {
                    Console.WriteLine($"[Email] Porta 465 falhou, tentando porta 587 com STARTTLS...");
                    await cliente.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                }
                
                Console.WriteLine($"[Email] Conectado! Autenticando...");
                await cliente.AuthenticateAsync(_email, _appPassword);
                
                Console.WriteLine($"[Email] Autenticado! Enviando mensagem...");
                
                // Desabilita pipelining para evitar problemas
                cliente.Capabilities &= ~SmtpCapabilities.Pipelining;
                
                await cliente.SendAsync(mensagem);
                
                Console.WriteLine($"[Email] ✅ Email enviado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email] ❌ Erro ao enviar: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Email] ❌ Erro interno: {ex.InnerException.Message}");
                }
                throw;
            }
            finally
            {
                if (cliente.IsConnected)
                {
                    await cliente.DisconnectAsync(true);
                    Console.WriteLine($"[Email] Desconectado do servidor SMTP");
                }
            }
        }

        // Helper para remover HTML e criar versão texto
        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            
            // Remove tags HTML básicas
            var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            // Remove espaços múltiplos
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }

        // Email de boas-vindas ao cadastrar morador
        public async Task EnviarEmailBoasVindas(string nomeMorador, string emailMorador)
        {
            var assunto = "Bem-vindo ao PortSafe!";
            var corpo = $@"Olá {nomeMorador}!

Seu cadastro foi realizado com sucesso no PortSafe!

Agora você pode aproveitar todas as facilidades do sistema:

- Receber notificações quando suas entregas chegarem
- Acessar armários inteligentes com senha exclusiva
- Ter total controle das suas encomendas

Em caso de dúvidas, entre em contato com a portaria do seu condomínio.

Atenciosamente,
Equipe PortSafe

---
Este é um e-mail automático. Por favor, não responda.
© 2025 PortSafe - Sistema de Gestão de Entregas";

            await EnviarAsync(emailMorador, assunto, corpo);
        }

        // Email de reset de senha
        public async Task EnviarEmailResetSenha(string nomeMorador, string emailMorador, string codigoReset)
        {
            var assunto = "Redefinição de Senha - PortSafe";
            var corpo = $@"Olá {nomeMorador}!

Você solicitou a redefinição de senha para sua conta no PortSafe.

Use o código abaixo para redefinir sua senha:

============================
      {codigoReset}
============================

⚠️ ATENÇÃO: Este código é válido por 1 hora.

Se você não solicitou esta redefinição, ignore este e-mail.

Atenciosamente,
Equipe PortSafe

---
PortSafe - Sistema de Gestão de Entregas";

            await EnviarAsync(emailMorador, assunto, corpo);
        }

        // Email de notificação de entrega no armário
        public async Task EnviarEmailEntregaArmario(string nomeMorador, string emailMorador, string numeroArmario, string senhaAcesso, string codigoEntrega)
        {
            var assunto = "Sua entrega chegou - PortSafe";
            
            // Texto simples sem HTML
            var corpoHtml = $@"
Olá {nomeMorador}!

Sua encomenda chegou e está disponível para retirada.

INFORMAÇÕES DA ENTREGA:
- Armário: {numeroArmario}
- Senha: {senhaAcesso}
- Código: {codigoEntrega}

COMO RETIRAR:
1. Vá até o armário {numeroArmario}
2. Digite a senha {senhaAcesso}
3. Retire sua encomenda
4. Feche bem a porta

Retire sua encomenda o mais breve possível.

Atenciosamente,
Equipe PortSafe
";

            await EnviarAsync(emailMorador, assunto, corpoHtml);
        }
    }
}