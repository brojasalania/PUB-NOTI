using Newtonsoft.Json;
using NotificationWin.View;
using PubnubApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NotificationWin.Model
{
    public class PubNubHelper
    {

        private readonly string ChannelName = "win-notification";
        Pubnub pubnub;
        public void Init()
        {
            //Init
            PNConfiguration pnConfiguration = new PNConfiguration
            {
                PublishKey = "pub-c-258022ec-24f8-496d-86f5-2f04ab0a8c0a",
                SubscribeKey = "sub-c-36b786be-0635-11ea-acc8-1a72d7432d4b",
                Secure = true //or true ?
            };
            pubnub = new Pubnub(pnConfiguration);

            //Subscribe
            pubnub.Subscribe<string>()
           .Channels(new string[] {
               ChannelName
           })
           .WithPresence() //no withpresence
           .Execute();
        }

        //Publish a message
        public void Publish()
        {
            JsonMsg Person = new JsonMsg
            {
                Name = "John Doe",
                Description = "Hello World",
                Date = DateTime.Now.ToString()
            };

            //Convert to string
            string arrayMessage = JsonConvert.SerializeObject(Person);

            pubnub.Publish()
                .Channel(ChannelName)
                .Message(arrayMessage)
                .Execute(new PNPublishResultExt((result, status) => { }));
        }

        //listen to the channel
        public void Listen()
        {
            SubscribeCallbackExt listenerSubscribeCallack = new SubscribeCallbackExt(
            (pubnubObj, message) => {


                //Call the notification windows from the UI thread
                Application.Current.Dispatcher.Invoke(new Action(() => {
                    //Show the message as a WPF window message like WIN-10 toast
                    NofiticationWindow ts = new NotificationWindow();

                    //Convert the message to JSON
                    JsonMsg bsObj = JsonConvert.DeserializeObject<JsonMsg>(message.Message.ToString());

                    string messageBoxText = "Name: " + bsObj.Name + Environment.NewLine + "Description: " + bsObj.Description + Environment.NewLine + "Date: " + bsObj.Date;
                    ts.NotifText.Text = messageBoxText;
                    ts.Show();
                }));
            },
            (pubnubObj, presence) => {
                // handle incoming presence data
            },
            (pubnubObj, status) => {
                // the status object returned is always related to subscribe but could contain
                // information about subscribe, heartbeat, or errors
                // use the PNOperationType to switch on different options
            });

            pubnub.AddListener(listenerSubscribeCallack);
        }
    }
}
