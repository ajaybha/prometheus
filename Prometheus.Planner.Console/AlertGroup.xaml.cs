﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Prometeo.Planner.Console.Tools;
using Prometeo.Planner.Console.ViewModel;
using Prometeo.Planner.Console.ViewModel.Alerts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prometeo.Planner.Console
{
    /// <summary>
    /// Interaction logic for AlertGroup.xaml
    /// </summary>
    public partial class AlertGroup : Fluent.RibbonWindow, INotifyPropertyChanged
    {
        public ApplicationCommand CmdActivated { get; set; }
        public ApplicationCommand CmdAdd { get; set; }
        public ApplicationCommand CmdRemove { get; set; }
        public ApplicationCommand CmdCancel { get; set; }
        public List<AlertGroups> AllMembers { get; set; }
        public AlertGroups SelectedMember { get; set; }
        public Visibility AllMembersVisibility { get; set; }

        BackgroundWorker _worker;
        public AlertGroup()
        {
            InitializeComponent();
            DataContext = this;

            CmdAdd = new ApplicationCommand(CmdAdd_Execute);
            CmdRemove = new ApplicationCommand(CmdRemove_Execute);
            CmdCancel = new ApplicationCommand(CmdCancel_Execute);

            _worker = new BackgroundWorker();
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            AllMembersVisibility = Visibility.Visible;
            NotifyPropertyChanged("AllMembersVisibility");
            NotifyPropertyChanged("AllMembers");
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<AlertGroups> blogs = new List<AlertGroups>();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["BlogConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable alertsTable = tableClient.GetTableReference("AlertGroups");

            try
            {
                var query = new TableQuery<AlertGroups>();
                AllMembers = alertsTable.ExecuteQuery(query).ToList();
            }
            catch { }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            AllMembersVisibility = Visibility.Hidden;
            NotifyPropertyChanged("AllMembersVisibility");

            if (!_worker.IsBusy)
                _worker.RunWorkerAsync();
        }

        private void CmdCancel_Execute(object obj)
        {
            Close();
        }

        private void CmdRemove_Execute(object obj)
        {
            if (SelectedMember != null)
                if (RESTTools.SimpleGet(ConfigurationManager.AppSettings["unsubscribeNumberToGroupAlert"].ToString(), SelectedMember.RowKey))
                    Window_Activated(null, null);
        }

        private void CmdAdd_Execute(object obj)
        {
            var partition = ConfigurationManager.AppSettings["subscribePartition"].ToString();

            if (Subscribe.AddNewUserToGroup(partition))
                Window_Activated(null, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
