namespace WebAPI
{
    public class FreshDeskContact
    {
        public bool active { get; set; }
        public string address { get; set; }
        public FreshDeskAvatar avatar { get; set; }
        //public int company_id { get; set; }
        public bool view_all_tickets { get; set; }
        public Dictionary<string, string>? custom_fields { get; set; }
        //public bool deleted { get; set; }
        public string? description { get; set; }
        public string?   email { get; set; }
        //public int id { get; set; }
        public string? job_title { get; set; }
        public string? language { get; set; }
        public string? mobile { get; set; }
        public string name { get; set; }
        public List<string>? other_emails { get; set; }
        public string? phone { get; set; }
        public List<string>? tags { get; set; }
        public string? time_zone { get; set; }
        public string? twitter_id { get; set; }
        public string unique_external_id { get; set; }
        public List<Dictionary<string, object>>? other_companies { get; set; }
        //public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }

        public FreshDeskContact(GitHubUser gitHubUser)
        {
            active = true;
            address = gitHubUser.location;
            name = gitHubUser.login;
            unique_external_id = gitHubUser.id.ToString();
            email = gitHubUser.login + "@github.com";
            job_title = "GitHub User (" + DateTime.Now.ToString("yyyyMMdd HH:mm") + ")";

            /*avatar = new FreshDeskAvatar();
            avatar.avatar_url = gitHubUser.avatar_url;
            avatar.content_type = "application/octet-stream";*/
        }
    }
}
