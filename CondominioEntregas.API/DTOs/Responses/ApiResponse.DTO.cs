public class ApiResponse<T>
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public T Dados { get; set; }
        public List<string> Erros { get; set; }

        public ApiResponse()
        {
            Erros = new List<string>();
        }
    }
