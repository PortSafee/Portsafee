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
            mensagem.From.Add(MailboxAddress.Parse(_email));
            mensagem.To.Add(MailboxAddress.Parse(para));
            mensagem.Subject = assunto;
            mensagem.Body = new TextPart("html") { Text = corpoHtml };

            using var cliente = new SmtpClient();
            
            try
            {
                // Configurações de timeout
                cliente.Timeout = 30000; // 30 segundos
                
                // Desabilita verificação de certificado SSL (útil em desenvolvimento)
                cliente.ServerCertificateValidationCallback = (s, c, h, e) => true;
                
                // Conecta com TLS
                await cliente.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                
                // Autentica com a senha de app
                await cliente.AuthenticateAsync(_email, _appPassword);
                
                // Envia com timeout
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await cliente.SendAsync(mensagem, cts.Token);
                
                Console.WriteLine("Email enviado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                if (cliente.IsConnected)
                {
                    await cliente.DisconnectAsync(true);
                }
            }
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
            var assunto = "📦 Sua entrega chegou! - PortSafe";
            var corpoHtml = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; border-radius: 5px; }}
                        .content {{ background-color: #f9f9f9; padding: 20px; margin-top: 20px; border-radius: 5px; }}
                        .info-box {{ background-color: #fff; border: 2px solid #2196F3; padding: 15px; margin: 15px 0; border-radius: 5px; }}
                        .destaque {{ background-color: #e3f2fd; padding: 10px; text-align: center; font-size: 20px; font-weight: bold; margin: 10px 0; border-radius: 5px; }}
                        .footer {{ margin-top: 20px; text-align: center; font-size: 12px; color: #777; }}
                        .importante {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 15px 0; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>📦 Sua Entrega Chegou!</h1>
                        </div>
                        <div class='content'>
                            <h2>Olá, {nomeMorador}!</h2>
                            <p>Temos uma ótima notícia! Sua encomenda foi depositada em um armário seguro e já está disponível para retirada.</p>
                            
                            <div class='info-box'>
                                <h3>📍 Informações da Entrega:</h3>
                                <p><strong>Armário:</strong></p>
                                <div class='destaque'>Nº {numeroArmario}</div>
                                
                                <p><strong>Senha de Acesso:</strong></p>
                                <div class='destaque'>{senhaAcesso}</div>
                                
                                <p><strong>Código de Rastreio:</strong> {codigoEntrega}</p>
                            </div>

                            <div class='importante'>
                                <strong>⚠️ Instruções para Retirada:</strong>
                                <ol>
                                    <li>Dirija-se até o armário número <strong>{numeroArmario}</strong></li>
                                    <li>Digite a senha <strong>{senhaAcesso}</strong> no painel</li>
                                    <li>Retire sua encomenda</li>
                                    <li>Feche bem a porta do armário</li>
                                </ol>
                            </div>

                            <p>⏰ Recomendamos que retire sua encomenda o mais breve possível.</p>
                            <p>Em caso de dúvidas ou problemas, entre em contato com a portaria.</p>
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
    }
}