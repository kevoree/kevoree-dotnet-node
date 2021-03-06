﻿namespace Org.Kevoree.Library

module BindingsOperations =

    let AddBinding:Org.Kevoree.Core.Api.IMarshalled.IMBindingMarshalled -> Org.Kevoree.Log.Api.ILogger -> Org.Kevoree.Library.AdaptationType.RegistryManager -> string -> bool = fun c logger registryManager nodeName ->
        let chanPath = c.getHub().path()
        if registryManager.Lookup(chanPath) then
            let chanInstance = registryManager.QueryRegistry(chanPath) :?> Org.Kevoree.Core.Api.IComponentRunner
            if chanInstance <> null then
                let compPath = c.getPort().eContainer().path()
                let compInstanceExists = registryManager.Lookup(compPath)
                if compInstanceExists then
                    let compInstance:Org.Kevoree.Core.Api.IComponentRunner = registryManager.QueryRegistry(compPath) :?> Org.Kevoree.Core.Api.IComponentRunner
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
                        chanInstance.attachInputPort(portInput);
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
                    if c.getPort().eContainer().path() = nodeName then
                        false
                    else
                        let isProvided = c.getPort().eContainer().CastToComponentInstance().findProvidedByID(c.getPort().getName())
                        if isProvided <> null then 
                            logger.Debug("Add an remote input in AddBinding Command")
                            let portInput:PortInput = if registryManager.Lookup(c.getPort().getName()) then
                                                            registryManager.QueryRegistry(c.getPort().path()) :?> PortInput
                                                        else 
                                                            let p:PortInput = new PortInput( c.getPort().getName(), c.getPort().path(), null)
                                                            let portPath = c.getPort().path()
                                                            registryManager.SaveToModel(portPath,p)
                                                            p
                            // todo : enregistrer le port input dans le channel
                            chanInstance.attachRemoteInputPort(portInput);
                        true
            else 
                logger.Error(sprintf "Instance %s not found" (c.getHub().path()))
                false
        else true

    let RemoveBinding:Org.Kevoree.Core.Api.IMarshalled.IMBindingMarshalled -> Org.Kevoree.Log.Api.ILogger -> Org.Kevoree.Library.AdaptationType.RegistryManager -> bool = fun c logger registryManager ->
        let chanPath = c.getHub().path()
        let chanInstance = registryManager.QueryRegistry(chanPath) :?> Org.Kevoree.Core.Api.IComponentRunner
        if chanInstance <> null then
            let compPath = c.getPort().eContainer().path()
            
            let existsCompInstance = registryManager.Lookup(compPath)
            if existsCompInstance then

                let compInstance:Org.Kevoree.Core.Api.IComponentRunner = registryManager.QueryRegistry(compPath) :?> Org.Kevoree.Core.Api.IComponentRunner
                let portPath = c.getPort().path()

                logger.Debug(sprintf "%s <-> %s <-> %s" compPath portPath chanPath)

                (* provided = INPUT*)
                (* required = OUTPUT *)
                let provided = c.getPort().eContainer().CastToComponentInstance().findProvidedByID(c.getPort().getName())
                if provided <> null then 
                    (* Input case *)
                    logger.Debug("Remove an input in RemoveBinding Command")
                    let portInput:PortInput = registryManager.QueryRegistry(c.getPort().path()) :?> PortInput
                    // todo : enregistrer le port input dans le channel
                    chanInstance.detachInputPort(portInput);
                    true
                else
                    (* Output case *)
                    logger.Debug("Remove an ouput in RemoveBinding Command")
                    let portOutput:PortOutput = registryManager.QueryRegistry(c.getPort().path()) :?> PortOutput
                    portOutput.detachChannel(chanInstance);
                    true
            else
                logger.Debug("Add an remote input in AddBinding Command")
                if registryManager.Lookup(c.getPort().path()) then
                    let portInput:PortInput = registryManager.QueryRegistry(c.getPort().path()) :?> PortInput
                    // todo : enregistrer le port input dans le channel
                    chanInstance.detachRemoteInputPort(portInput);
                true
        else 
            logger.Error(sprintf "Instance %s not found" (c.getHub().path()))
            false