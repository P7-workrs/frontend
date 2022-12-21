namespace p7_client_frontendV2.Data
{
    public class ClientData
    {
        public string ClientId = string.Empty;
        public string ServerName = string.Empty;
        public string DataServerName = string.Empty;
        
        public void Update(ResponseDTO ResponseDTO)  
        {
            ClientId = ResponseDTO.ClientId;
            ServerName = ResponseDTO.ServerName;
            DataServerName = ResponseDTO.DataServerName;
        }
    }

}
