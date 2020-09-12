using CraftopiaMerger.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CraftopiaMerger
{
    public partial class MainWindow : Form
    {
        private string _sourceFilePath = string.Empty;
        private string _destionationFilePath = string.Empty;

        private FileInfo _sourceFile;
        private FileInfo _destionationFile;

        private JObject _sourceData;
        private JObject _destionationData;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonOpenSource_Click(object sender, EventArgs e)
        {
            _sourceFilePath = openOcsFile();
            textBoxSource.Text = _sourceFilePath;

            if(string.Empty != _sourceFilePath)
            {
                _sourceFile = new FileInfo(_sourceFilePath);
                var sourceRawData = Decompress(_sourceFile);
                _sourceData = JObject.Parse(sourceRawData);
                updatelistBoxSource();
            }
        }

        private void buttonOpenDestionation_Click(object sender, EventArgs e)
        {
            _destionationFilePath = openOcsFile();
            textBoxDestionation.Text = _destionationFilePath;

            if (string.Empty != _destionationFilePath)
            {
                _destionationFile = new FileInfo(_destionationFilePath);
                var destionationRawData = Decompress(_destionationFile);
                _destionationData = JObject.Parse(destionationRawData);
                updatelistBoxDestionation();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            saveOcsFile(_destionationData.ToString());
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (null != _sourceData && null != _destionationData)
            {
                var selectedMap = listBoxSource.SelectedItem.ToString();

                addMap(selectedMap, _sourceData, _destionationData);
                updatelistBoxDestionation();
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if(null != _sourceData && null != _destionationData)
            { 
                var selectedMap = listBoxDestionation.SelectedItem.ToString();
                removeMap(_destionationData, selectedMap);
            }
        }

        private void updatelistBoxSource()
        {
            var maps = getMaps(_sourceData);
            listBoxSource.Items.Clear();
            listBoxSource.Items.AddRange(maps.ToArray());
        }

        private void updatelistBoxDestionation()
        {
            var maps = getMaps(_destionationData);
            listBoxDestionation.Items.Clear();
            listBoxDestionation.Items.AddRange(maps.ToArray());
        }

        private List<string> getMaps(JObject jsonData)
        {
            var maps = getMapsObject(jsonData);

            var listBoxItems = new List<string>();

            foreach(var map in maps)
            {
                listBoxItems.Add(map["name"].ToString());
            }

            return listBoxItems;
        }

        private bool mapExists(JObject jsonData, string mapName)
        {
            var maps = getMapsObject(jsonData);

            foreach (var map in maps)
            {
                if(map["name"].ToString() == mapName)
                {
                    return true;
                }
            }

            return false;
        }

        private JToken getMapsObject(JObject jsonData)
        {
            return jsonData["WorldListSave"]["value"];
        }

        private void appendToMapsObject(JObject jsonData, JToken map)
        {
            jsonData["WorldListSave"]["value"].Value<JArray>().Add(map);
        }

        private void removeMap(JObject jsonData, string mapName)
        {
            var maps = getMapsObject(jsonData);

            var removeMaps = new List<JToken>();

            foreach (var map in maps)
            {
                if (map["name"].ToString() == mapName)
                {
                    removeMaps.Append(map);
                }
            }

            foreach(var map in removeMaps)
            {
                map.Remove();
            }
        }

        private JToken getMapObject(string mapName, JObject jsonData)
        {
            var maps = getMapsObject(jsonData);

            foreach (var map in maps)
            {
                if (map["name"].ToString() == mapName)
                {
                    return map;
                }
            }

            return null;
        }

        private bool addMap(string mapName, JObject jsonDataSource, JObject jsonDataDestination)
        {
            var map = getMapObject(mapName, jsonDataSource);

            if(mapExists(jsonDataDestination, mapName))
            {
                return false;
            }

            appendToMapsObject(jsonDataDestination, map);
            return true;
        }

        private string openOcsFile()
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Craftopia Save Files (*.ocs)|*.ocs";
                openFileDialog.FilterIndex = 0;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }

            return string.Empty;
        }

        private void saveOcsFile(string destinationData)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Craftopia Save Files (*.ocs)|*.ocs";
                saveFileDialog.Title = "Save Craftopia Save File";
                saveFileDialog.ShowDialog();

                var saveFileName = saveFileDialog.FileName;
                if (string.Empty != saveFileName)
                {
                    var saveFileInfo = new FileInfo(saveFileName);
                    Compress(saveFileInfo, destinationData);
                }
            }
        }

        private void Compress(FileInfo fileToCompress, string destionationData)
        {
            using (var fileToCompressStream = destionationData.ToMemoryStream())
            { 
                using (var compressedFileStream = File.Create(fileToCompress.FullName))
                {
                    using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        fileToCompressStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        private string Decompress(FileInfo fileToDecompress)
        {
            using (var originalFileStream = fileToDecompress.OpenRead())
            {
                using (var decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                {
                    using (var stringStream = new StreamReader(decompressionStream))
                    {
                        return stringStream.ReadToEnd();
                    }
                }
            }

            return string.Empty;
        }
    }
}
