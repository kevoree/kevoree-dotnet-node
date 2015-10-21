namespace Org.Kevoree.Library

open Org.Kevoree.Core.Api.IMarshalled
open Org.Kevoree.Log.Api

type UpdateDictionaryCommand(c:IInstanceMarshalled, dicValue:IValueMarshalled, nodeName:string, registry:ModelRegistry, bs:Org.Kevoree.Core.Api.BootstrapService, modelService:Org.Kevoree.Core.Api.ModelService, logger:ILogger) =
    inherit System.MarshalByRefObject()
    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            logger.Debug("Execute UpdateDictionary")
            let path = c.path()
            let lookup = registry.ContainsKey(path)
            if lookup then
                let component = registry.[path] :?> Org.Kevoree.Core.Api.IComponentRunner
                let attributeDefinition = dicValue.eContainer().eContainer().CastToInstance().GetTypeDefinition().getDictionaryType().findAttributesByID(dicValue.getName());
                component.updateDictionary(attributeDefinition, dicValue);
                true
            else false
        member this.Undo() = 
            logger.Debug("Undo UpdateDictionay")
            ()
        member this.Name() = sprintf "[UpdateDictionnary nodeName=%s]" nodeName