using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using Microsoft.TeamFoundation.Build.Workflow.Tracking;

namespace BuildTasks.Activities
{
    [BuildActivity(HostEnvironmentOption.All)]
    public sealed class SvnActivity : CodeActivity
    {
        public InArgument<string> SvnToolPath { get; set; }

        public InArgument<string> SvnCommandArgs { get; set; }

        public InArgument<string> DestinationPath { get; set; }

        public InArgument<string> SvnPath { get; set; }

        public InArgument<SvnCredentials> SvnCredentials { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            TrackMessage(context, "Starting SVN action");

            string destinationPath = context.GetValue(this.DestinationPath);
            string svnPath = context.GetValue(this.SvnPath);
            string svnToolPath = context.GetValue(this.SvnToolPath);
            string svnCommandArgs = context.GetValue(this.SvnCommandArgs);
            SvnCredentials svnCredentials = context.GetValue(this.SvnCredentials);

            string svnCommand = Regex.Replace(svnCommandArgs, "{uri}", svnPath);
            svnCommand = Regex.Replace(svnCommand, "{destination}", destinationPath);
            svnCommand = Regex.Replace(svnCommand, "{username}", svnCredentials.Username);
            TrackMessage(context, "svn command: " + svnCommand);

            // Never reveal the password!
            svnCommand = Regex.Replace(svnCommand, "{password}", svnCredentials.Password);

            if (File.Exists(svnToolPath))
            {
                var process = Process.Start(svnToolPath, svnCommand);
                if (process != null)
                {
                    process.WaitForExit();
                    process.Close();
                }
            }

            TrackMessage(context, "End SVN action");
        }

        private void TrackMessage(CodeActivityContext context, string message)
        {
            context.Track(new BuildInformationRecord<BuildMessage>
            {
                Value = new BuildMessage()
                {
                    Importance = BuildMessageImportance.Normal,
                    Message = message,
                },
            });
        }

        public sealed class BuildMessage
        {
            public String Message { get; set; }
            public BuildMessageImportance Importance { get; set; }
        }
    }

    public sealed class SvnCredentials
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
