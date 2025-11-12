using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using PortSafe.Data;
using PortSafe.DTOs;
using PortSafe.Models;
using System.Security.Claims;
using System.Text;


namespace PortSafe.Services
{
    public class AuthService
    {
        private readonly PortSafeContext _context;
        private readonly IConfiguration _config;

        public AuthService(PortSafeContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }



        // Método de cadastro
        public async Task<Morador?> Cadastro(CadastroMoradorRequestDTO request)
        {
            // Verifica se os campos obrigatórios estão preenchidos
            if (string.IsNullOrWhiteSpace(request.Nome) ||
                string.IsNullOrWhiteSpace(request.Senha) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                request.CondominioId <= 0)
            {
                throw new ArgumentException("Nome, Senha, Email e Condomínio são obrigatórios.");
            }

            // Verifica se pelo menos um tipo de unidade foi fornecido
            bool temDadosCasa = request.DadosCasa != null && 
                               !string.IsNullOrWhiteSpace(request.DadosCasa.Rua);
            
            bool temDadosApartamento = request.DadosApartamento != null && 
                                      !string.IsNullOrWhiteSpace(request.DadosApartamento.Bloco);

            if (!temDadosCasa && !temDadosApartamento)
            {
                throw new ArgumentException("Informe os dados da sua Casa ou Apartamento.");
            }

            // Verifica se ambos os tipos foram fornecidos (não permitido)
            if (temDadosCasa && temDadosApartamento)
            {
                throw new ArgumentException("Informe apenas um tipo de unidade (Casa OU Apartamento).");
            }

            // Verifica se o email já está em uso
            var moradorExistente = await _context.Moradores
                .FirstOrDefaultAsync(m => m.Email == request.Email);

            if (moradorExistente != null)
            {
                throw new ArgumentException("Email já está cadastrado.");
            }

            // Busca o condomínio e verifica o tipo
            var condominio = await _context.Condominios
                .FirstOrDefaultAsync(c => c.Id == request.CondominioId);

            if (condominio == null)
            {
                throw new ArgumentException($"Condomínio com ID {request.CondominioId} não existe.");
            }

            // Valida se o tipo de unidade corresponde ao tipo de condomínio
            bool isCondCasa = condominio.Tipo == "Casa";
            bool isCondApartamento = condominio.Tipo == "Apartamento";

            // Criação da unidade baseada no tipo de condomínio
            Unidade? novaUnidade = null;

            if (isCondCasa && temDadosCasa) // Se for casa, dados da casa são obrigatórios
            {
                // Valida dados da casa
                if (string.IsNullOrWhiteSpace(request.DadosCasa!.Rua) ||
                    request.DadosCasa.NumeroCasa <= 0 ||
                    string.IsNullOrWhiteSpace(request.DadosCasa.CEP))
                {
                    throw new ArgumentException("Rua, Número da Casa e CEP são obrigatórios.");
                }

                novaUnidade = new UnidadeCasa
                {
                    CondominioId = request.CondominioId,
                    Rua = request.DadosCasa.Rua,
                    NumeroCasa = request.DadosCasa.NumeroCasa,
                    CEP = request.DadosCasa.CEP,
                    DataCriacao = DateTime.UtcNow
                };

                _context.UnidadesCasa.Add((UnidadeCasa)novaUnidade);
            }
            else if (isCondApartamento && temDadosApartamento) // Se for apartamento, dados do apto são obrigatórios
            {
                // Valida dados do apartamento
                if (string.IsNullOrWhiteSpace(request.DadosApartamento!.Bloco) ||
                    string.IsNullOrWhiteSpace(request.DadosApartamento.NumeroApartamento))
                {
                    throw new ArgumentException("Torre e Número do Apartamento são obrigatórios.");
                }

                novaUnidade = new UnidadeApartamento
                {
                    CondominioId = request.CondominioId,
                    Torre = request.DadosApartamento.Bloco,
                    NumeroApartamento = request.DadosApartamento.NumeroApartamento,
                    DataCriacao = DateTime.UtcNow
                };

                _context.UnidadesApartamento.Add((UnidadeApartamento)novaUnidade);
            }
            else if (isCondCasa && temDadosApartamento)
            {
                throw new ArgumentException("Este é um condomínio de casas. Informe os dados da casa.");
            }
            else if (isCondApartamento && temDadosCasa)
            {
                throw new ArgumentException("Este é um condomínio de apartamentos. Informe os dados do apartamento.");
            }

            // Salva a unidade para obter o ID
            await _context.SaveChangesAsync();

            // Criação do novo morador
            var novoMorador = new Morador
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha), // Hash da senha
                CondominioId = request.CondominioId,
                UnidadeId = novaUnidade?.Id, // Associa a unidade criada
                Telefone = request.Telefone,
                CPF = request.CPF,
                Photo = request.Photo,
                DataCriacao = DateTime.UtcNow
            };

            _context.Moradores.Add(novoMorador);
            await _context.SaveChangesAsync();

            return novoMorador;
        }



        // Método de login
        public Morador? Login(string email, string senha)
        {
            var usuario = _context.Moradores.FirstOrDefault(u => u.Email == email); // Busca o usuário pelo email
            if (usuario == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash)) return null;
            return usuario;
        }



        // Método de cadastro de Porteiro (Simples)
        public async Task<Porteiro?> CadastroPorteiro(CadastroPorteiroRequestDTO request)
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(request.Nome) ||
                string.IsNullOrWhiteSpace(request.Senha) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Telefone) ||
                request.CondominioId <= 0)
            {
                throw new ArgumentException("Nome, Senha, Email, Telefone e Condomínio são obrigatórios.");
            }

            // Verifica se o email já está em uso
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuarioExistente != null)
            {
                throw new ArgumentException("Email já está cadastrado.");
            }

            // Verifica se o condomínio existe
            var condominio = await _context.Condominios
                .FirstOrDefaultAsync(c => c.Id == request.CondominioId);

            if (condominio == null)
            {
                throw new ArgumentException($"Condomínio com ID {request.CondominioId} não existe. Por favor, verifique o ID do condomínio.");
            }

            // Cria o novo porteiro
            var novoPorteiro = new Porteiro
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                Telefone = request.Telefone,
                CondominioId = request.CondominioId,
                Tipo = TipoUsuario.Porteiro,
                DataCriacao = DateTime.UtcNow
            };

            _context.Porteiros.Add(novoPorteiro);
            await _context.SaveChangesAsync();

            return novoPorteiro;
        }



        // Método para gerar o token JWT

        public string GenerateJwtToken(Usuario usuario, TipoUsuario tipo) 
        {
            var role = tipo switch // Define a role com base no tipo de usuário
            {
                TipoUsuario.Morador => "Morador",
                TipoUsuario.Porteiro => "Porteiro",
                _ => "Morador"
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "Chave Secreta")); // Chave secreta para assinar o token

            var claims = new[] 
            {
                new Claim (JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),  
                new Claim (JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty), 
                new Claim (ClaimTypes.Role, role) 
            };

            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); 

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"], 
                claims: claims,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token); // Gera o token como string
        }


        // Método para solicitar reset de senha
        public async Task<string> SolicitarResetSenha(string email)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null)
            {
                // Por segurança, não informar que o email não existe
                return string.Empty;
            }

            // Gera um token de 6 dígitos
            var random = new Random();
            var token = random.Next(100000, 999999).ToString();

            usuario.ResetToken = token;
            usuario.ResetTokenExpiracao = DateTime.UtcNow.AddHours(1); // Token válido por 1 hora

            await _context.SaveChangesAsync();

            // Aqui você pode enviar o token por email
            // Por enquanto, apenas retorna o token (em produção, deve enviar por email)
            return token;
        }


        // Método para redefinir senha
        public async Task<bool> RedefinirSenha(string email, string token, string novaSenha)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email);

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
        
        

    }
}

