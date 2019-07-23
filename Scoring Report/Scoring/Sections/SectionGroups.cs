﻿using Scoring_Report.Configuration;
using Scoring_Report.Configuration.Groups;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scoring_Report.Scoring.Sections
{
    public class SectionGroups : ISection
    {
        public string Header => "Groups:";

        public const string GroupFormat = "Group \"{0}\" correctly configured - {1}";

        public int MaxScore()
        {
            // Set max to 0
            int max = 0;

            // For each group settings in list
            foreach (GroupSettings group in ConfigurationManager.Groups)
            {
                // If group is scored, increment max
                if (group.IsScored) max++;
            }

            return max;
        }

        public SectionDetails GetScore()
        {
            SectionDetails details = new SectionDetails(0, new List<string>(), this);

            // If no configuration for this section, return empty details
            if (ConfigurationManager.Groups.Count == 0) return details;

            // Create instance for communicating with active directory
            using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
            {
                // Create searcher for active directory
                using (PrincipalSearcher searcher = new PrincipalSearcher(new GroupPrincipal(context)))
                {
                    // For each group on the machine
                    foreach (GroupPrincipal group in searcher.FindAll())
                    {
                        // For each group config
                        foreach (GroupSettings settings in ConfigurationManager.Groups)
                        {
                            // If enumerated settings is not for specified group, skip
                            if (group.Name != settings.GroupName)
                            {
                                continue;
                            }

                            // If number of members of group and config are inequal, skip
                            if (group.Members.Count != settings.Members.Count)
                            {
                                continue;
                            }

                            // Copy the list so we may remove elements
                            List<UserPrincipal> membersGroup = new List<UserPrincipal>(group.Members.Cast<UserPrincipal>());

                            bool groupsMatch = true;

                            // For each member config
                            foreach (IMember memberConfig in settings.Members)
                            {
                                // Member found that matches current config
                                UserPrincipal foundMember = null;

                                // For each member in the group
                                foreach (UserPrincipal member in membersGroup)
                                {
                                    string identifier = "";

                                    // Determine the id type and get identifier
                                    switch (memberConfig.IDType)
                                    {
                                        case "Username":
                                            identifier = member.SamAccountName; 
                                            break;
                                        case "SID":
                                            identifier = member.Sid.Value;
                                            break;
                                    }

                                    // If identifier matches config
                                    if (identifier == memberConfig.Identifier)
                                    {
                                        foundMember = member;
                                        break;
                                    }
                                }

                                // If no match was found, groups do not match
                                if (foundMember == null)
                                {
                                    groupsMatch = false;
                                    break;
                                }

                                // Member was found, remove from list
                                // for optimization and possible duplicates
                                membersGroup.Remove(foundMember);
                            }

                            // If groups match, output and give point
                            if (groupsMatch)
                            {
                                details.Points++;

                                // Get list of members' names separated by commas
                                string members = string.Join(", ", group.Members.Cast<UserPrincipal>().Select(x => x.SamAccountName));

                                details.Output.Add(string.Format(GroupFormat, settings.GroupName, members));
                            }
                        }
                    }
                }
            }
            return details;
        }
    }
}