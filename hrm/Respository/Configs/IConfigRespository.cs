namespace hrm.Respository.Configs
{
    public interface IConfigRespository
    {
        public Task<Entities.Configs?> UploadLogo(int id, string url);
    }
}
