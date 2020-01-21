using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using findawayworld;
using System.IO;

namespace ScribdDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //allowed input
            //366626161
            //https://www.scribd.com/audiobook/366626161/Influence-The-Psychology-of-Persuasion
            //https://www.scribd.com/listen/366626161

            var listenPageHtml = await GetListenPageHtml("366626161");
            var book = GetBookDetails(listenPageHtml);
            var playlist = await GetPlaylist(book.audiobook);
            await DownloadPlaylist(GetSafeFileName(book.doc.title), playlist);


            Console.WriteLine("Hello World!");
        }

        private static async Task DownloadPlaylist(string title, Playlist playlist)
        {
            using var client = new HttpClient();
            var numberOfParts = playlist.playlist.Length;
            Console.WriteLine("Downloading: " + title);
            Console.WriteLine("Found items: " + numberOfParts);
            var partNumber = 0;
            foreach (var item in playlist.playlist)
            {
                partNumber++;
                var fileName = partNumber.ToString().PadLeft(2, '0') + " - " + title + ".mp3";
                Console.Write(fileName);
                using var remoteFile = await client.GetStreamAsync(item.url);
                using var localFile = File.OpenWrite(fileName);
                remoteFile.CopyTo(localFile);
                Console.WriteLine(" - DONE");

                //using (var memoryStream = new MemoryStream())
                //{
                //    await file.CopyToAsync(memoryStream);
                //    return memoryStream.ToArray();
                //}
            }
        }

        private static string GetSafeFileName(string file)
        {
            Array.ForEach(Path.GetInvalidFileNameChars(),
                  c => file = file.Replace(c.ToString(), String.Empty));
            return file;
        }
        private static async Task<findawayworld.Playlist> GetPlaylist(Scribd.Audiobook audiobook)
        {
            var accountsUrl = $"https://api.findawayworld.com/v4/accounts/scribd-495688303/audiobooks/{audiobook.external_id}";
            var playlistsUrl = $"https://api.findawayworld.com/v4/audiobooks/{audiobook.external_id}/playlists";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Session-Key", audiobook.session_key);
            Console.Write("Downloading findawayworld metadata");
            var accountJson = await client.GetStringAsync(accountsUrl);
            var account = JsonSerializer.Deserialize<findawayworld.Account>(accountJson);
            var playlistResult = await client.PostAsync(playlistsUrl, new StringContent("{\"license_id\":\"" + account.licenses[0].id + "\"}"));
            var playlistJson = await playlistResult.Content.ReadAsStringAsync();
            var playlist = JsonSerializer.Deserialize<findawayworld.Playlist>(playlistJson);
            Console.WriteLine(" - DONE");
            return playlist;
        }

        private static Scribd.Book GetBookDetails(string listenPageHtml)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(listenPageHtml);
            var script = doc.DocumentNode.SelectNodes("//script").Single(s => s.Attributes["src"] == null && s.InnerHtml.Contains("external_id")).InnerHtml;
            script = script.Substring(script.IndexOf("ReactDOM.render(React.createElement(Scribd.Audiobooks.Show, ") + "ReactDOM.render(React.createElement(Scribd.Audiobooks.Show, ".Length);
            var json = script.Substring(0, script.IndexOf("), document.getElementById("));
            var book = System.Text.Json.JsonSerializer.Deserialize<Scribd.Book>(json);
            Console.WriteLine("Found book: " + book.doc.title);
            return book;
        }

        private static async Task<string> GetListenPageHtml(string bookId)
        {
            Console.Write("Downloading Scribd book details page");
            var url = "https://www.scribd.com/listen/" + bookId;
            var cookieContainer = new CookieContainer();

            //cookieContainer.Add(new Cookie("scribd_ubtc", "u%3D50654427-f74d-4cd9-b1a8-a9f65f12a456%26h%3DXvhelICrgpFlCI8Zq8x0WNiZE2ejKG%2BbJhreuT5wCUQ%3D", domain: ".scribd.com", path: "/"));
            //cookieContainer.Add(new Cookie("_scribd_expire", "1579598447", domain: ".scribd.com", path: "/"));
            //cookieContainer.Add(new Cookie("_scribd_session", "eyJzZXNzaW9uX2lkIjoiZTEyMGM1MmUzNDMyNjBkOThlZTg5YjliZDljNTE0OGMiLCJyIjoiMTU3OTU5ODQ0NyIsIndvcmRfaWQiOjQ5NTY4ODMwMywicCI6MTU3OTU5NDE1MSwibGFzdF9yZWF1dGgiOjE1Nzk1OTg0NDd9--f4127c5365c7fb8a2b9ee0999179b3a802e24c2f", domain: ".scribd.com", path: "/"));
            using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            using var client = new HttpClient(handler);
            var loginUrl = "https://www.scribd.com/login";
            //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            var loginResult = await client.PostAsync(loginUrl, new StringContent("{\"login_or_email\":\"mikeon@objectware.pl\",\"login_password\":\"Dupatej.2\",\"rememberme\":\"\",\"signup_location\":\"https://www.scribd.com/\",\"login_params\":{}}", System.Text.Encoding.UTF8, "application/json"));
            var html = await client.GetStringAsync(url);

            Console.WriteLine(" - DONE");
            return html;
        }
    }
}
namespace Scribd
{ 
    public class Book
    {
        public string eor_url { get; set; }
        public Share_Opts share_opts { get; set; }
        public Save_Button save_button { get; set; }
        public bool finished { get; set; }
        public string gradient_color { get; set; }
        public Credit_Data credit_data { get; set; }
        public Doc doc { get; set; }
        public Audiobook audiobook { get; set; }
        public Save_Prompt save_prompt { get; set; }
        public Saved saved { get; set; }
        public Save_For_Later_Promo save_for_later_promo { get; set; }
        public Bookmark bookmark { get; set; }
        public Preview_Finished preview_finished { get; set; }
        public Preview_Roadblock_Copy preview_roadblock_copy { get; set; }
        public Repeat_Preview_Title repeat_preview_title { get; set; }
        public string[] pingback_url { get; set; }
        public string logo_url { get; set; }
        public float ios_version { get; set; }
        public string preview_url { get; set; }
        public string pmp_document_purchases_url { get; set; }
        public string dunning_lock_status_url { get; set; }
        public Restriction_Flags restriction_flags { get; set; }
    }

    public class Share_Opts
    {
        public int id { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string message { get; set; }
        public string twitter_message { get; set; }
        public string description { get; set; }
        public string thumbnail_url { get; set; }
        public string mailto_url { get; set; }
    }

    public class Save_Button
    {
        public Item item { get; set; }
        public string cta_remove { get; set; }
        public string cta_save { get; set; }
        public bool show_icon { get; set; }
        public bool show_text { get; set; }
        public bool show_tooltip { get; set; }
        public bool text_btn { get; set; }
        public bool use_thick_icon { get; set; }
    }

    public class Item
    {
        public int id { get; set; }
        public bool in_library { get; set; }
        public string short_title { get; set; }
    }

    public class Credit_Data
    {
        public int preview_threshold { get; set; }
        public int payout_threshold { get; set; }
        public bool preview_restricted { get; set; }
        public bool scribd_select { get; set; }
    }

    public class Doc
    {
        public int id { get; set; }
        public object canonical_doc_id { get; set; }
        public string title { get; set; }
        public string cover_url { get; set; }
        public string cover_url_retina { get; set; }
        public string url { get; set; }
        public string document_type { get; set; }
        public bool is_snapshot { get; set; }
        public Author author { get; set; }
        public Narrator narrator { get; set; }
    }

    public class Author
    {
        public string url { get; set; }
        public string name { get; set; }
    }

    public class Narrator
    {
        public string url { get; set; }
        public string name { get; set; }
    }

    public class Audiobook
    {
        public string account_id { get; set; }
        public string session_key { get; set; }
        public bool debug { get; set; }
        public string external_id { get; set; }
    }

    public class Save_Prompt
    {
        public Already_Saved already_saved { get; set; }
        public int document_id { get; set; }
        public string header { get; set; }
        public string location { get; set; }
        public string save_text { get; set; }
        public string save_url { get; set; }
        public bool user_is_subscriber { get; set; }
    }

    public class Already_Saved
    {
        public int id { get; set; }
        public int word_user_id { get; set; }
        public int word_document_id { get; set; }
        public string state { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime interacted_at { get; set; }
    }

    public class Saved
    {
        public int id { get; set; }
        public int word_user_id { get; set; }
        public int word_document_id { get; set; }
        public string state { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime interacted_at { get; set; }
    }

    public class Save_For_Later_Promo
    {
        public string header { get; set; }
        public string subheader { get; set; }
        public Document_Is_Saved document_is_saved { get; set; }
        public bool user_is_subscriber { get; set; }
    }

    public class Document_Is_Saved
    {
        public int id { get; set; }
        public int word_user_id { get; set; }
        public int word_document_id { get; set; }
        public string state { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime interacted_at { get; set; }
    }

    public class Bookmark
    {
        public int id { get; set; }
        public string state { get; set; }
    }

    public class Preview_Finished
    {
        public string title { get; set; }
        public object next_issue_thumbnail_url { get; set; }
        public object next_issue_title { get; set; }
    }

    public class Preview_Roadblock_Copy
    {
        public string pre_link_text { get; set; }
        public string link_text { get; set; }
        public string post_link_text { get; set; }
    }

    public class Repeat_Preview_Title
    {
        public string title { get; set; }
        public string content { get; set; }
    }

    public class Restriction_Flags
    {
        public Flag flag { get; set; }
        public Availability availability { get; set; }
    }

    public class Flag
    {
        public string type { get; set; }
        public string label { get; set; }
        public string classes { get; set; }
    }

    public class Availability
    {
        public string icon { get; set; }
        public string text { get; set; }
        public object date { get; set; }
    }
}
namespace findawayworld
{
    public class Account
    {
        public License[] licenses { get; set; }
        public Audiobook audiobook { get; set; }
    }

    public class Audiobook
    {
        public object[] series { get; set; }
        public Chapter[] chapters { get; set; }
        public string sample_url { get; set; }
        public string id { get; set; }
        public string abridgement { get; set; }
        public string grade_level { get; set; }
        public string copyright { get; set; }
        public string sub_title { get; set; }
        public DateTime modified_date { get; set; }
        public string[] bisac_codes { get; set; }
        public bool drm_free { get; set; }
        public string description { get; set; }
        public string publisher { get; set; }
        public bool chapterized { get; set; }
        public string cover_url { get; set; }
        public string metadata_sig { get; set; }
        public object[] awards { get; set; }
        public string[] authors { get; set; }
        public string[] narrators { get; set; }
        public int actual_size { get; set; }
        public string language { get; set; }
        public string title { get; set; }
        public string street_date { get; set; }
        public string runtime { get; set; }
    }

    public class Chapter
    {
        public int duration { get; set; }
        public int part_number { get; set; }
        public int chapter_number { get; set; }
    }

    public class License
    {
        public DateTime modified_date { get; set; }
        public string content_id { get; set; }
        public string product_id { get; set; }
        public string business_model { get; set; }
        public string territory { get; set; }
        public string id { get; set; }
        public string account_id { get; set; }
    }


    public class Playlist
    {
        public PlaylistItem[] playlist { get; set; }
        public DateTime expires { get; set; }
        public string playlist_token { get; set; }
    }

    public class PlaylistItem
    {
        public string url { get; set; }
        public int part_number { get; set; }
        public int chapter_number { get; set; }
    }


}
