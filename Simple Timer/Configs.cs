using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Simple_Timer
{
    public class Configs
    {
        public int TimerFontSize { get; set; }
        public bool Mo3Toggle { get; set; }
        public bool Ao5Toggle { get; set; }
        public bool Ao12Toggle { get; set; }
        public bool ScrambleToggle { get; set; }
        public bool InspectionToggle { get; set; }
        public bool AcrylicToggle { get; set; }

        public void SetToggles(bool isOn = false)
        {
            Mo3Toggle = isOn;
            Ao5Toggle = isOn;
            Ao12Toggle = isOn;
            InspectionToggle = isOn;
            ScrambleToggle = isOn;
            AcrylicToggle = isOn;
        }

        public async Task Save()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);

            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(".config");

            StorageFile file;
            if (item is null)
            {
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(".config",
                    CreationCollisionOption.ReplaceExisting);
            }
            else
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(".config");
            }
            await FileIO.WriteTextAsync(file, jsonString);
        }

        public async Task Load()
        {
            string jsonString;
            Configs tmpThis;
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(".config");
                jsonString = await FileIO.ReadTextAsync(file);
                tmpThis = JsonSerializer.Deserialize<Configs>(jsonString);
            }
            catch (Exception)
            {
                this.TimerFontSize = 72;
                SetToggles();
                await Save();
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(".config");
                jsonString = await FileIO.ReadTextAsync(file);
                tmpThis = JsonSerializer.Deserialize<Configs>(jsonString);
            }

            this.TimerFontSize = tmpThis.TimerFontSize;
            this.Mo3Toggle = tmpThis.Mo3Toggle;
            this.Ao5Toggle = tmpThis.Ao5Toggle;
            this.Ao12Toggle = tmpThis.Ao12Toggle;
            this.InspectionToggle = tmpThis.InspectionToggle;
            this.ScrambleToggle = tmpThis.ScrambleToggle;
            this.AcrylicToggle = tmpThis.AcrylicToggle;
        }
    }
}
