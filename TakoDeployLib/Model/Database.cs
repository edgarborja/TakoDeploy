﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TakoDeployCore.DataContext;

namespace TakoDeployCore.Model
{
    public abstract class Database : Notifier, IDisposable
    {

        private TakoDbContext _context = null;
        [JsonIgnore]
        public TakoDbContext Context
        {
            get
            {
                if (_context == null)
                {
                    var factory = new TakoConnectionFactory();
                    //var task = factory.GetDbContextAsync(ProviderName, ConnectionString);
                    //_context = task.Result;
                    _context = factory.GetDbContext(ProviderName, ConnectionString);
                    _context.InfoMessage += (sender, e) => { OnInfoMessage(sender, e); }; // this is sexy
                }
                return _context;
            }
        }

        public virtual void OnInfoMessage(object sender, SqlInfoMessageEventArgs e) { }

        private string _deploymentStatus = null;
        public string DeploymentStatusMessage { get { return _deploymentStatus; } internal set { SetField(ref _deploymentStatus, value); } }

        private DatabaseDeploymentState _state = 0;
        public DatabaseDeploymentState DeploymentState { get { return _state; } internal set { SetField(ref _state, value); } }

        public enum DatabaseDeploymentState
        {
            Idle = 0,
            Starting , 
            Success,
            Error ,
            Cancelled 
        }

        private string _name;
        public string Name { get { return _name; } set { SetField(ref _name, value); } }

        private bool _checked;
        public bool Checked { get { return _checked; } set { SetField(ref _checked, value); } }

        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }

        public async Task<bool> TryConnect(CancellationToken ct)
        {
            var result = false;
            try
            { 
                await Context.OpenAsync(ConnectionString, ct);
                result = Context.IsOpen();
                //Context.FinishConnection();
            }
            catch (Exception ex)
            {
                DeploymentStatusMessage = (ex.InnerException?? ex)?.Message; //OH YEA
            }
            return result;
        }

        public void Dispose()
        {
            if (Context != null)
            {
                Context.CloseConnections();
            }
        }
    }
   
}
