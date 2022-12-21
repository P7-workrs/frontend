namespace p7_client_frontendV2.Data
{
    public class DummyWorkers
    {
        public string _jobId;
        public string _jobName;
        public string _status;
       
        public DummyWorkers(string jobId,string jobName, string status) 
        {
            
            _jobId= jobId;
            _jobName = jobName;
            _status = status;
        }

        public string Job_ID { get { return _jobId; } }
        public string Job_Name { get { return _jobName; } }
        public string Status { get { return _status;} }

    }
}
