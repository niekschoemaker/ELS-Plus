using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ELS_Server
{
    public class VcfSync
    {

        public static List<string> ElsResources = new List<string>();
        public static List<Tuple<string, string, string>> VcfData = new List<Tuple<string, string, string>>();

        public async Task CheckVCF(Player player)
        {
            foreach (string name in ElsResources)
            {
                LoadFilesPromScript(name);
            }
        }

        public static async Task CheckResources()
        {
            var numResources = Function.Call<int>(Hash.GET_NUM_RESOURCES);
            for (int x = 0; x < numResources; x++)
            {
                var name = Function.Call<string>(Hash.GET_RESOURCE_BY_FIND_INDEX, x);
                string isElsResource = API.GetResourceMetadata(name, "is_els", 0);
                if (!String.IsNullOrEmpty(isElsResource) && isElsResource.Equals("true"))
                {
                    ElsResources.Add(name);
                    LoadFilesPromScript(name);
                }

            }
            Utils.ReleaseWriteLine($"Total ELS Resources: {ElsResources.Count}");
        }

        internal static async void LoadFilesPromScript(string name)
        {
            int num = Function.Call<int>(Hash.GET_NUM_RESOURCE_METADATA, name, "file");

            for (int i = 0; i < num; i++)
            {
                var filename = Function.Call<string>(Hash.GET_RESOURCE_METADATA, name, "file", i);

                var data = Function.Call<string>(Hash.LOAD_RESOURCE_FILE, name, filename);
                if (Path.GetExtension(filename).ToLower() == ".xml")
                {
                    try
                    {
                        if (VCF.isValidData(data))
                        {
                            VcfData.Add(new Tuple<string, string, string>(name, filename, data));
                        }
                    }
                    catch (XMLParsingException e)
                    {
                        Utils.ReleaseWriteLine($"There was a parsing error in {filename} please validate this XML and try again.");
                    }
                }
            }
        }
    }
}
