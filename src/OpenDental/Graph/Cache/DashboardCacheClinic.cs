using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness;

namespace OpenDental.Graph.Cache
{
    public class DashboardCacheClinic : DashboardCacheBase<ClinicDto>
    {
        private Dictionary<long, string> _dictClinicNames = new Dictionary<long, string>();

        protected override List<ClinicDto> GetCache(DashboardFilter filter)
        {
            var list = Clinics.GetDeepCopy();
            
            _dictClinicNames = list.ToDictionary(x => x.Id, x => string.IsNullOrEmpty(x.Description) ? x.Id.ToString() : x.Description);
            
            return list;
        }

        public string GetClinicName(long clinicNum)
        {
            if (_dictClinicNames.TryGetValue(clinicNum, out var clinicName)) return clinicName;
            
            return clinicNum == 0 ? "Unassigned" : clinicNum.ToString();
        }
    }
}