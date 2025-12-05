using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PortSafe.Data;
using PortSafe.DTOs;
using PortSafe.Models;

namespace PortSafe.Services
{
    public class AuthService
    {
        private readonly PortSafeContext _context;
        private readonly IConfiguration _config;
        private readonly GmailService _gmailService;

        public AuthService(PortSafeContext context, IConfiguration config, GmailService gmailService)
        {
            _context = context;
            _config = config;
            _gmailService = gmailService;
        }

        // ========================
        // CADASTRO DE MORADOR
        // ========================
        public async Task<Morador> Cadastro(CadastroMoradorRequestDTO request)
        {
            ValidarCamposObrigatoriosMorador(request);

            var condominio = await ValidarCondominioAsync(request.CondominioId);
            await ValidarEmailUnicoAsync<Morador>(request.Email);

            var novaUnidade = await CriarUnidadeAsync(request, condominio);
            await _context.SaveChangesAsync(); // Salva unidade para gerar ID

            var morador = new Morador
            {
                Nome = request.Nome.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                CondominioId = request.CondominioId,
                UnidadeId = novaUnidade.Id,
                Telefone = request.Telefone?.Trim(),
                CPF = request.CPF?.Trim(),
                Photo = request.Photo,
                DataCriacao = DateTime.UtcNow
            };

            _context.Moradores.Add(morador);
            await _context.SaveChangesAsync();

            return morador;
        }

        // ========================
        // CADASTRO DE PORTEIRO
        // ========================
        public async Task<Porteiro> CadastroPorteiro(CadastroPorteiroRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Nome) ||
                string.IsNullOrWhiteSpace(request.Senha) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Telefone) ||
                request.CondominioId <= 0)
            {
                throw new ArgumentException("Nome, Senha, Email, Telefone e Condomínio são obrigatórios.");
            }

            await ValidarCondominioAsync(request.CondominioId);

            var emailJaUsado = await _context.Moradores.AnyAsync(m => m.Email == request.Email) ||
                               await _context.Porteiros.AnyAsync(p => p.Email == request.Email);

            if (emailJaUsado)
                throw new ArgumentException("Email já está cadastrado.");

            var porteiro = new Porteiro
            {
                Nome = request.Nome.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                Telefone = request.Telefone.Trim(),
                CondominioId = request.CondominioId,
                Tipo = TipoUsuario.Porteiro,
                DataCriacao = DateTime.UtcNow
            };

            _context.Porteiros.Add(porteiro);
            await _context.SaveChangesAsync();

            return porteiro;
        }

        // ========================
        // LOGIN
        // ========================
        public async Task<Usuario?> LoginAsync(string email, string senha)
        {
            email = email.Trim().ToLowerInvariant();

            var morador = await _context.Moradores.FirstOrDefaultAsync(m => m.Email == email);
            if (morador != null && BCrypt.Net.BCrypt.Verify(senha, morador.SenhaHash))
                return morador;

            var porteiro = await _context.Porteiros.FirstOrDefaultAsync(p => p.Email == email);
            if (porteiro != null && BCrypt.Net.BCrypt.Verify(senha, porteiro.SenhaHash))
                return porteiro;

            return null;
        }

        // ========================
        // GERAÇÃO DE JWT
        // ========================
        public string GenerateJwtToken(Usuario usuario, TipoUsuario tipo)
        {
            var role = tipo == TipoUsuario.Porteiro ? "Porteiro" : "Morador";

            var keyString = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString) || keyString.Length < 16)
                throw new InvalidOperationException("Chave JWT não configurada ou muito curta.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ========================
        // RESET DE SENHA
        // ========================
        public async Task<string> SolicitarResetSenha(string email)
        {
            email = email.Trim().ToLowerInvariant();

            var usuario = await BuscarUsuarioPorEmailAsync(email);
            if (usuario == null)
                return string.Empty;

            var token = new Random().Next(100000, 999999).ToString();

            usuario.ResetToken = token;
            usuario.ResetTokenExpiracao = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            // Envia e-mail de reset
            await _gmailService.EnviarEmailResetSenha(usuario.Nome ?? "Usuário", usuario.Email, token);

            return token;
        }

        public async Task<bool> RedefinirSenha(string email, string token, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6)
                return false;

            email = email.Trim().ToLowerInvariant();
            var usuario = await BuscarUsuarioPorEmailAsync(email);

            if (usuario == null ||
                usuario.ResetToken != token ||
                usuario.ResetTokenExpiracao == null ||
                usuario.ResetTokenExpiracao < DateTime.UtcNow)
            {
                return false;
            }

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
            usuario.ResetToken = null;
            usuario.ResetTokenExpiracao = null;

            await _context.SaveChangesAsync();
            return true;
        }

        // ========================
        // MÉTODOS PRIVADOS AUXILIARES
        // ========================
        private async Task<Condominio> ValidarCondominioAsync(int condominioId)
        {
            var condominio = await _context.Condominios
                .FirstOrDefaultAsync(c => c.Id == condominioId);

            if (condominio == null)
                throw new ArgumentException($"Condomínio com ID {condominioId} não existe.");

            return condominio;
        }

        private async Task ValidarEmailUnicoAsync<T>(string email) where T : class
        {
            email = email.Trim().ToLowerInvariant();
            bool existe = _context.Set<T>().Any(e => EF.Property<string>(e, "Email") == email);
            if (existe)
                throw new ArgumentException("Email já está cadastrado.");
        }

        private void ValidarCamposObrigatoriosMorador(CadastroMoradorRequestDTO r)
        {
            if (string.IsNullOrWhiteSpace(r.Nome) ||
                string.IsNullOrWhiteSpace(r.Senha) ||
                string.IsNullOrWhiteSpace(r.Email) ||
                r.CondominioId <= 0)
            {
                throw new ArgumentException("Nome, Senha, Email e Condomínio são obrigatórios.");
            }

            bool temCasa = r.DadosCasa != null && !string.IsNullOrWhiteSpace(r.DadosCasa.Rua);
            bool temApto = r.DadosApartamento != null && !string.IsNullOrWhiteSpace(r.DadosApartamento.Bloco);

            if (temCasa && temApto)
                throw new ArgumentException("Informe apenas um tipo de unidade (Casa OU Apartamento).");

            if (!temCasa && !temApto)
                throw new ArgumentException("Informe os dados da sua Casa ou Apartamento.");
        }

        private async Task<Unidade> CriarUnidadeAsync(CadastroMoradorRequestDTO request, Condominio condominio)
        {
            bool isCasa = condominio.Tipo == "Casa";
            bool isApto = condominio.Tipo == "Apartamento";

            if (isCasa && request.DadosApartamento != null && !string.IsNullOrWhiteSpace(request.DadosApartamento.Bloco))
                throw new ArgumentException("Este é um condomínio de casas. Informe os dados da casa.");

            if (isApto && request.DadosCasa != null && !string.IsNullOrWhiteSpace(request.DadosCasa.Rua))
                throw new ArgumentException("Este é um condomínio de apartamentos. Informe os dados do apartamento.");

            if (isCasa)
            {
                if (request.DadosCasa == null ||
                    string.IsNullOrWhiteSpace(request.DadosCasa.Rua) ||
                    request.DadosCasa.NumeroCasa <= 0 ||
                    string.IsNullOrWhiteSpace(request.DadosCasa.CEP))
                {
                    throw new ArgumentException("Rua, Número da Casa e CEP são obrigatórios para condomínio de casas.");
                }

                var unidade = new UnidadeCasa
                {
                    CondominioId = request.CondominioId,
                    Rua = request.DadosCasa.Rua.Trim(),
                    NumeroCasa = request.DadosCasa.NumeroCasa,
                    CEP = request.DadosCasa.CEP.Trim(),
                    DataCriacao = DateTime.UtcNow
                };

                _context.UnidadesCasa.Add(unidade);
                await _context.SaveChangesAsync();
                return unidade;
            }
            else
            {
                if (request.DadosApartamento == null ||
                    string.IsNullOrWhiteSpace(request.DadosApartamento.Bloco) ||
                    string.IsNullOrWhiteSpace(request.DadosApartamento.NumeroApartamento))
                {
                    throw new ArgumentException("Torre/Bloco e Número do Apartamento são obrigatórios.");
                }

                var unidade = new UnidadeApartamento
                {
                    CondominioId = request.CondominioId,
                    Torre = request.DadosApartamento.Bloco.Trim(),
                    NumeroApartamento = request.DadosApartamento.NumeroApartamento.Trim(),
                    DataCriacao = DateTime.UtcNow
                };

                _context.UnidadesApartamento.Add(unidade);
                await _context.SaveChangesAsync();
                return unidade;
            }
        }

        private async Task<Usuario?> BuscarUsuarioPorEmailAsync(string email)
        {
            email = email.Trim().ToLowerInvariant();

            var morador = await _context.Moradores.FirstOrDefaultAsync(m => m.Email == email);
            if (morador != null) return morador;

            var porteiro = await _context.Porteiros.FirstOrDefaultAsync(p => p.Email == email);
            return porteiro;
        }
    }
}
