﻿using Microsoft.Win32;
using Scoring_Report.Configuration;
using System.Collections.Generic;
using System.IO;

namespace Scoring_Report.Scoring.Sections
{
    class SectionShares : ISection
    {
        public ESectionType Type => ESectionType.Shares;

        public string Header => "Shares:";

        public static Dictionary<string, bool> LocalShares { get; } = new Dictionary<string, bool>();

        public int MaxScore()
        {
            return LocalShares.Count;
        }

        public SectionDetails GetScore()
        {
            SectionDetails details = new SectionDetails(0, new List<string>(), this);

            // Loop over all shares
            foreach (KeyValuePair<string, bool> share in LocalShares)
            {
                if (ShareExists(share.Key) == share.Value)
                {
                    details.Points++;
                    details.Output.Add(TranslationManager.Translate("Shares", share.Key, share.Value ? 
                        TranslationManager.Translate("Exists") : TranslationManager.Translate("Deleted")));
                }
            }

            return details;
        }

        public void Load(BinaryReader reader)
        {
            LocalShares.Clear();

            // Get count of scored shares
            int sharecount = reader.ReadInt32();

            for (int i = 0; i < sharecount; i++)
            {
                // Get Share Name
                string sharename = reader.ReadString();

                // Scoring status
                bool isScored = reader.ReadBoolean();

                // Score if exists
                bool exists = reader.ReadBoolean();

                if (!isScored) continue;

                LocalShares.Add(sharename, exists);
            }
        }

        public static bool ShareExists(string sharename)
        {
            try
            {
                // Searching the registry to see if a share under the given name is scored
                string name = "SYSTEM\\CurrentControlSet\\services\\LanmanServer\\Shares";
                using (RegistryKey hklm = Registry.LocalMachine.OpenSubKey(name))
                {
                    if (hklm != null)
                    {
                        if (hklm.GetValue(sharename) != null)
                        {
                            // If GetValue is not null, then the share exists
                            return true;
                        }
                    }
                }
                return false;
            }
            catch { return false; }
        }
    }
}
