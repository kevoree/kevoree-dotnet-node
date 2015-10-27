namespace Org.Kevoree.Library

open Org.Kevoree.Log.Api
open Org.Kevoree.Library.AdaptationType

type AddBindingCommand(c:Org.Kevoree.Core.Api.IMarshalled.IMBindingMarshalled, nodeName:string, registryManager:Org.Kevoree.Library.AdaptationType.RegistryManager, nodePath:string ,logger:ILogger) =
    inherit System.MarshalByRefObject()

    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            logger.Debug("Executed AddBinding")
            let chanPath = c.getHub().path()
            let chanInstance = registryManager.QueryRegistry(chanPath) :?> Org.Kevoree.Core.Api.IComponentRunner
            if chanInstance <> null then
                let compPath = c.getPort().eContainer().path()
                let compInstance:Org.Kevoree.Core.Api.IComponentRunner = registryManager.QueryRegistry(compPath) :?> Org.Kevoree.Core.Api.IComponentRunner
                if compInstance <> null then
                    let portPath = c.getPort().path()

                    logger.Debug(sprintf "%s <-> %s <-> %s" compPath portPath chanPath)

                    (* provided = INPUT*)
                    (* required = OUTPUT *)
                    let provided = c.getPort().eContainer().CastToComponentInstance().findProvidedByID(c.getPort().getName())
                    if provided <> null then 
                        (* Input case *)
                        logger.Debug("Add an input in AddBinding Command")
                        let portInput:PortInput = if registryManager.Lookup(portPath) then
                                                        registryManager.QueryRegistry(c.getPort().path()) :?> PortInput
                                                    else 
                                                        let p:PortInput = new PortInput( c.getPort().getName(), c.getPort().path(), compInstance)
                                                        let portPath = c.getPort().path()
                                                        registryManager.SaveToModel(portPath,p)
                                                        p
                        // todo : enregistrer le port input dans le channel
                        chanInstance.attachInputPort(portInput, c.getPort().getName());
                        true
                    else
                        (* Output case *)
                        logger.Debug("Add an ouput in AddBinding Command")
                        let portOutput:PortOutput = if registryManager.Lookup portPath then
                                                        registryManager.QueryRegistry(c.getPort().path()) :?> PortOutput
                                                    else 
                                                        let p:PortOutput = new PortOutput(c.getPort().getName(), c.getPort().path())
                                                        registryManager.SaveToModel (c.getPort().path(),  p)
                                                        p
                        portOutput.registerChannel(chanInstance);
                        compInstance.attachOutputPort(portOutput, c.getPort().getName())
                        true
                else
                    (*if c.getPort().eContainer().path() = nodeName then
                        false
                    else
                        let isProvided = c.getPort().eContainer().CastToComponentInstance().findProvidedByID(c.getPort().getName())
                        if isProvided <> null then 
                            let pi = new PortInput(c.getPort().getName(), c.getPort().path(), chanInstance);
                            registry.Add(c.getPort().path(), pi)
                            pi.registerComponent()*)
                    logger.Error("Unhandled case in AddBinding Command")
                    false
            else 
                logger.Error(sprintf "Instance %s not found" (c.getHub().path()))
                false
        member this.Undo() = 
            logger.Debug("Undo AddBinding")
            let cmd = new RemoveBindingCommand(c, nodeName, registryManager, logger)
            let _ = (cmd :>  Org.Kevoree.Core.Api.Command.ICommand).Execute()
            ()
        member this.Name() = sprintf "[AddBinding nodeName=%s]" nodeName