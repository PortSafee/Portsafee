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
            var corpoHtml = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
                        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; border-radius: 5px; }}
                        .footer {{ margin-top: 20px; text-align: center; font-size: 12px; color: #777; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🏠 Bem-vindo ao PortSafe!</h1>
                        </div>
                        <div class='content'>
                            <h2>Olá, {nomeMorador}!</h2>
                            <p>Seu cadastro foi realizado com sucesso em nosso sistema.</p>
                            <p>Agora você pode aproveitar todas as facilidades do PortSafe para receber suas entregas de forma segura e prática.</p>
                            <h3>O que você pode fazer:</h3>
                            <ul>
                                <li>✅ Receber notificações quando suas entregas chegarem</li>
                                <li>✅ Acessar armários inteligentes com senha exclusiva</li>
                                <li>✅ Ter total controle das suas encomendas</li>
                            </ul>
                            <p>Em caso de dúvidas, entre em contato com a portaria do seu condomínio.</p>
                        </div>
                        <div class='footer'>
                            <p>Este é um e-mail automático. Por favor, não responda.</p>
                            <p>&copy; 2025 PortSafe - Sistema de Gestão de Entregas</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            await EnviarAsync(emailMorador, assunto, corpoHtml);
        }

        // Email de reset de senha
        public async Task EnviarEmailResetSenha(string nomeMorador, string emailMorador, string codigoReset)
        {
            var assunto = "Redefinição de Senha - PortSafe";
            var corpoHtml = $@"
                <html>
                <body style='font-family: Arial, sans-serif; padding: 20px;'>
                    <h2 style='color: #FF9800;'>🔑 Redefinição de Senha</h2>
                    <p>Olá, <strong>{nomeMorador}</strong>!</p>
                    <p>Você solicitou a redefinição de senha para sua conta no PortSafe.</p>
                    <p>Use o código abaixo para redefinir sua senha:</p>
                    <div style='background: #f0f0f0; padding: 15px; text-align: center; font-size: 28px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                        {codigoReset}
                    </div>
                    <p style='color: #d32f2f;'><strong>⚠️ Este código é válido por 30 minutos.</strong></p>
                    <p>Se você não solicitou esta redefinição, ignore este e-mail.</p>
                    <hr style='margin: 20px 0;'>
                    <p style='font-size: 12px; color: #777;'>PortSafe - Sistema de Gestão de Entregas</p>
                </body>
                </html>
            ";

            await EnviarAsync(emailMorador, assunto, corpoHtml);
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