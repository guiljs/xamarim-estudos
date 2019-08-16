using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using App2.Views;
using App2.Models;
using Android.Hardware;
using Android.Provider;
using Android.Graphics;
using System.IO;
using RestSharp;

namespace App2
{
    [Activity(Label = "App2", MainLauncher = true)]
    public class MainActivity : Activity
    {
        int count = 1;
        private ImageView imageView;
        private EditText txtHost;
        private Button btCamera;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var webView = FindViewById<WebView>(Resource.Id.webView);
            webView.Settings.JavaScriptEnabled = true;

            // Use subclassed WebViewClient to intercept hybrid native calls
            webView.SetWebViewClient(new HybridWebViewClient());

            // Render the view from the type generated from RazorView.cshtml
            var model = new Model1() { Text = "Text goes here" };
            var template = new RazorView() { Model = model };
            var page = template.GenerateString();

            // Load the rendered HTML into the view with a base URL 
            // that points to the root of the bundled Assets folder
            webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);

            var botao = FindViewById<Button>(Resource.Id.button1);
            var btGoToFoto = FindViewById<Button>(Resource.Id.btGoToFoto);
            var ratingBar = FindViewById<RatingBar>(Resource.Id.ratingBar1);
            var imagem = FindViewById<ImageView>(Resource.Id.imageView1);



            
            ratingBar.RatingBarChange += (o, e) =>
            {
                if (ratingBar.Rating > 5)
                {
                    imagem.SetImageResource(Resource.Drawable.miojo);
                }
                else
                {
                    imagem.SetImageResource(Resource.Drawable.pao_de_queijo);
                }
            };

            botao.Click += (o, e) =>
            {
                //botao.Text = $"{count++}"; ratingBar.Rating = count;

                //imagem.SetImageResource(Resource.Drawable.pao_de_queijo);

                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.layout1);

                var seekBar = FindViewById<SeekBar>(Resource.Id.seekBar1);

                seekBar.ProgressChanged += (ob, ev) =>
                {
                    var texto = FindViewById<TextView>(Resource.Id.textView1);
                    texto.Text = $"Valor é {seekBar.Progress}";
                };

            };

            btGoToFoto.Click += BtGoToFoto_Click;





        }

        private void BtGoToFoto_Click(object sender, EventArgs e)
        {
            SetContentView(Resource.Layout.Foto);
            btCamera = FindViewById<Button>(Resource.Id.btCamera);
            btCamera.Click += BtCamera_Click;

            imageView = FindViewById<ImageView>(Resource.Id.imageView);
            txtHost = FindViewById<EditText>(Resource.Id.editText1);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Bitmap bitmap = (Bitmap)data.Extras.Get("data");

            imageView.SetImageBitmap(bitmap);
            EnviaImagemParaPDF(bitmap);

        }

        private void EnviaImagemParaPDF(Bitmap bitmap)
        {
            MemoryStream stream = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, stream);



            var base64 = Convert.ToBase64String(stream.ToArray());

            var html = $"<h2>Imagem gerado a partir de foto do aplicativo</h2>" +
                $"<img src='data:image/png;base64, {base64}' />";

            //    html = Android.Text.Html.EscapeHtml(html);
            var host = $"{txtHost.Text}.ngrok.io";
            var client = new RestClient($"http://{host}/gerapdf");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Content-Length", "7");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Cookie", "ASP.NET_SessionId=tdfad1vazppuwxq52mwo35pq");
            request.AddHeader("Host", host);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("User-Agent", "PostmanRuntime/7.15.2");
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(html);
            //request.AddParameter("undefined", $"\"{html}\"", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

        }

        private void BtCamera_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            StartActivityForResult(intent, 0);
        }

        private class HybridWebViewClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView webView, string url)
            {

                // If the URL is not our own custom scheme, just let the webView load the URL as usual
                var scheme = "hybrid:";

                if (!url.StartsWith(scheme))
                    return false;

                // This handler will treat everything between the protocol and "?"
                // as the method name.  The querystring has all of the parameters.
                var resources = url.Substring(scheme.Length).Split('?');
                var method = resources[0];
                var parameters = System.Web.HttpUtility.ParseQueryString(resources[1]);

                if (method == "UpdateLabel")
                {
                    var textbox = parameters["textbox"];

                    // Add some text to our string here so that we know something
                    // happened on the native part of the round trip.
                    var prepended = string.Format("C# says \"{0}\"", textbox);

                    // Build some javascript using the C#-modified result
                    var js = string.Format("SetLabelText('{0}');", prepended);

                    webView.LoadUrl("javascript:" + js);
                }

                return true;
            }
        }
    }
}

