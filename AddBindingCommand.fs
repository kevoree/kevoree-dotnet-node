namespace Org.Kevoree.Library

open Org.Kevoree.Log.Api

type AddBindingCommand(c:Org.Kevoree.Core.Api.IMarshalled.IMBindingMarshalled, nodeName:string, registry:ModelRegistry, nodePath:string ,logger:ILogger) =
    inherit System.MarshalByRefObject()

    interface Org.Kevoree.Core.Api.Command.ICommand with
        member this.Execute() =
            logger.Debug("Executed AddBinding")
            let chanPath = c.getHub().path()
            let chanInstance = registry.[chanPath] :?> Org.Kevoree.Core.Api.IComponentRunner
            if chanInstance <> null then
                let compPath = c.getPort().eContainer().path()
                let compInstance:Org.Kevoree.Core.Api.IComponentRunner = registry.[compPath] :?> Org.Kevoree.Core.Api.IComponentRunner
                if compInstance <> null then
                    let portPath = c.getPort().path()

                    logger.Debug(sprintf "%s <-> %s <-> %s" compPath portPath chanPath)

                    (* provided = INPUT*)
                    (* required = OUTPUT *)
                    let provided = c.getPort().eContainer().CastToComponentInstance().findProvidedByID(c.getPort().getName())
                    if provided <> null then 
                        (* Input case *)
                        logger.Debug("Add an input in AddBinding Command")
                        let portInput:PortInput = if registry.ContainsKey(portPath) then
                                                        registry.[c.getPort().path()] :?> PortInput
                                                    else 
                                                        let p:PortInput = new PortInput( c.getPort().getName(), c.getPort().path(), chanInstance)
                                                        registry.Add(c.getPort().path(), p)
                                                        p
                        portInput.registerComponent(compInstance, c.getPort().getName())
                        true
                    else
                        logger.Debug("Add an ouput in AddBinding Command")
                        let portOutput:PortOutput = if registry.ContainsKey(portPath) then
                                                        registry.[c.getPort().path()] :?> PortOutput
                                                    else 
                                                        let methodName = ""
                                                        let p:PortOutput = new PortOutput(c.getPort().getName(), c.getPort().path(), compInstance :?> Org.Kevoree.Core.Api.ChannelDispatch, c.getPort().getName())
                                                        registry.Add(c.getPort().path(), p)
                                                        p
                        compInstance.attacheOutputPort(portOutput, c.getPort().getName())
                        (*
                        
                        TODO : Deal with PortOutput
                        1 - inject to a component field
                        2 - register every channel into the port
                        3 - Implement business code in each Port type

                        compInstance.addInternalOutputPort(portInstance)

                        let bindings = c.getHub().getBindings();
                        for binding in bindings do
                            if binding <> c then
                                let provided = binding.getPort().eContainer().CastToComponentInstance().findProvidedByID(binding.getPort().getName())
                                if provided <> null then
                                    let portInstance:PortImpl = if registry.ContainsKey(provided.path()) then 
                                                                    registry.[provided.path()] :?> PortImpl
                                                                else
                                                                    let tmp = new PortImpl(provided.getName(), provided.path())
                                                                    registry.Add(provided.path(), tmp)
                                                                    tmp
                                    chanInstance.addInternalInputPort(portInstance)
                          *)    
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
            let cmd = new RemoveBindingCommand(c, nodeName, registry, logger)
            let _ = (cmd :>  Org.Kevoree.Core.Api.Command.ICommand).Execute()
            ()
        member this.Name() = sprintf "[AddBinding nodeName=%s]" nodeName