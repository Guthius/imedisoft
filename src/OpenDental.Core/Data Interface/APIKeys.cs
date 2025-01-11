using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public static class APIKeys
{
    private static List<ApiKeyVisibleInfo> _apiKeyVisibleInfos;
    
    public static void SaveListApiKeys(List<APIKey> apiKeys)
    {
        Db.NonQ("DELETE FROM apikey");
        
        foreach (var apiKey in apiKeys)
        {
            Insert(apiKey);
        }
    }

    public static void Insert(APIKey apiKey)
    {
        APIKeyCrud.Insert(apiKey);
    }
    
    public static List<ApiKeyVisibleInfo> GetListApiKeyVisibleInfos(bool forceRefresh = false)
    {
        if (_apiKeyVisibleInfos != null && !forceRefresh)
        {
            return _apiKeyVisibleInfos;
        }

        _apiKeyVisibleInfos = [];

        var stringBuilder = new StringBuilder();
        var officeData = PayloadHelper.CreatePayload(stringBuilder.ToString(), eServiceCode.FHIR);
        
        var result = WebServiceMainHQProxy.GetWebServiceMainHQInstance().GetFHIRAPIKeysForOffice(officeData);

        var xmlDocument = new XmlDocument();
        
        xmlDocument.LoadXml(result);
        
        var xPathNavigator = xmlDocument.CreateNavigator();

        var xPathNavigatorNode = xPathNavigator.SelectSingleNode("//Error");
        if (xPathNavigatorNode != null)
        {
            throw new Exception(xPathNavigatorNode.Value);
        }

        xPathNavigatorNode = xPathNavigator.SelectSingleNode("//ListAPIKeys");
        if (xPathNavigatorNode == null || !xPathNavigatorNode.MoveToFirstChild())
        {
            return _apiKeyVisibleInfos;
        }

        do
        {
            var apiKeyVisibleInfo = new ApiKeyVisibleInfo();
            apiKeyVisibleInfo.CustomerKey = xPathNavigatorNode.SelectSingleNode("APIKeyValue").Value;
            apiKeyVisibleInfo.FHIRAPIKeyNum = SIn.Long(xPathNavigatorNode.SelectSingleNode("FHIRAPIKeyNum").Value);
            apiKeyVisibleInfo.DateDisabled = DateTime.Parse(xPathNavigatorNode.SelectSingleNode("DateDisabled").Value);
            
            if (!Enum.TryParse(xPathNavigatorNode.SelectSingleNode("KeyStatus").Value, out apiKeyVisibleInfo.FHIRKeyStatusCur))
            {
                apiKeyVisibleInfo.FHIRKeyStatusCur = Enum.TryParse(xPathNavigatorNode.SelectSingleNode("KeyStatus").Value, out APIKeyStatus status) 
                    ? FHIRUtils.ToFHIRKeyStatus(status) 
                    : FHIRKeyStatus.DisabledByHQ;
            }

            apiKeyVisibleInfo.DeveloperName = xPathNavigatorNode.SelectSingleNode("DeveloperName").Value;
            apiKeyVisibleInfo.DeveloperEmail = xPathNavigatorNode.SelectSingleNode("DeveloperEmail").Value;
            apiKeyVisibleInfo.DeveloperPhone = xPathNavigatorNode.SelectSingleNode("DeveloperPhone").Value;
            apiKeyVisibleInfo.FHIRDeveloperNum = SIn.Long(xPathNavigatorNode.SelectSingleNode("FHIRDeveloperNum").Value);
            
            var xPathNavigatorNodePerms = xPathNavigatorNode.SelectSingleNode("ListAPIPermissions");
            if (xPathNavigatorNodePerms == null || !xPathNavigatorNodePerms.MoveToFirstChild())
            {
                _apiKeyVisibleInfos.Add(apiKeyVisibleInfo);
                
                continue;
            }

            while (true)
            {
                if (Enum.TryParse(xPathNavigatorNodePerms.Value, out APIPermission apiPermission))
                {
                    apiKeyVisibleInfo.ListAPIPermissions.Add(apiPermission);
                }

                if (!xPathNavigatorNodePerms.MoveToNext())
                {
                    break;
                }
            }

            _apiKeyVisibleInfos.Add(apiKeyVisibleInfo);
        } while (xPathNavigatorNode.MoveToNext());

        return _apiKeyVisibleInfos;
    }
}