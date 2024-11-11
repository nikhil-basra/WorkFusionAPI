using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkFusionAPI.Interfaces;
using WorkFusionAPI.Models;
using WorkFusionAPI.Utility;

namespace WorkFusionAPI.Services
{
    public class AdminService : IAdminService
    {
        private readonly DBGateway _dbGateway;

        public AdminService(DBGateway dbGateway)
        {
            _dbGateway = dbGateway;
        }







    }

}
