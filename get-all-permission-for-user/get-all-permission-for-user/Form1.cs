using Newtonsoft.Json;


namespace get_all_permission_for_user
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
               

                string url = textBox1.Text;
                string user = textBox2.Text;
                string loginUser = textBox3.Text;
                string loginPassword = textBox4.Text;
                string file = textBox5.Text;

                if (user == "" || url == ""|| loginUser == ""|| file == "")
                {
                    label5.Text = "everything except password is required";
                    return;
                }
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                using var client = new HttpClient();


                string body = @$"<alm-authentication>
                                <user>{loginUser}</user>
                                <password>{loginPassword}</password>
                                </alm-authentication> ";

                HttpContent content = new StringContent(body);
                var auth = await client.PostAsync(url + "/authentication-point/alm-authenticate", content);
                if (!auth.IsSuccessStatusCode)
                {
                    throw new Exception(auth.StatusCode.ToString());
                }
                auth = await client.PostAsync(url + "/rest/site-session", content);
                auth = await client.GetAsync(url + $"/v2/sa/api/site-users/{user}/projects");

                var contentAsObject = auth.Content.ReadAsStringAsync().Result;

                var jsonString = await auth.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject<object>(jsonString);
                int pcount = result.projects.project.Count;
                for (int i = 0; i < pcount; i++)
                {
                    
                    var domain = result.projects.project[i]["domain-name"].ToString();
                    var project = result.projects.project[i].name.ToString();
                    label5.Text = $"Checking - {domain} {project}";
                    auth = await client.GetAsync(url + $"/v2/sa/api/domains/{domain}/projects/{project}/users/{user}/groups");
                    var finalRes = auth.Content.ReadAsStringAsync().Result;
                    dynamic desRes = JsonConvert.DeserializeObject<object>(finalRes);

                    //var groups = desRes.groups.ToString();
                    var groups = desRes.groups.group[0]["group-name"];
                    if (groups != "TDAdmin")
                    {
                        using (StreamWriter writer = new StreamWriter(file, true)) //true is to add into the file
                        {
                            writer.WriteLine(domain + " " + project + " " + groups);
                            
                        }
                    }
                }
                label5.Text = "done";
            }
            //label5.Text = result.projects.project[0].name.ToString();
            catch (Exception error)
            {
                label5.Text = error.Message;
            }

        }
    }
}