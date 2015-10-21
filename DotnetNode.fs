namespace Org.Kevoree.Library

open Org.Kevoree.Annotation
open Org.Kevoree.Core.Api.Handler
open Org.Kevoree.Core.Api
open Org.Kevoree.Core.Api.Adaptation
open org.kevoree
open org.kevoree.factory
open Org.Kevoree.Library.KevoreeKompareBean
open Org.Kevoree.Core.Api.IMarshalled

[<Org.Kevoree.Annotation.NodeType>]
type DotnetNode =
    class
        [<KevoreeInject>] val mutable modelService:ModelService

        [<KevoreeInject>] val mutable bootstrapService: BootstrapService

        [<KevoreeInject>] val mutable context: Org.Kevoree.Core.Api.Context 

        [<Param(Optional = true, DefaultValue = "INFO")>] val mutable logLevel:string
        
        [<KevoreeInject>] val mutable logger : Org.Kevoree.Log.Api.ILogger

        val mutable modelRegistry:ModelRegistry

        [<Start>]
        member this.Start() =
            this.logger.setLevel(this.logLevel);
            this.logger.Warn("Start");
            this.modelService.registerModelListener(this)
            this.modelRegistry.Add(this.context.getPath(), this)

        member this.Update() =
            this.logger.setLevel(this.logLevel);
            this.logger.Warn("Update");

        interface Org.Kevoree.Annotation.DeployUnit
        interface ModelListener with
            member this.preUpdate(context: UpdateContext): bool = false
            member this.initUpdate(context: UpdateContext): bool = false
            member this.afterLocalUpdate(context: UpdateContext): bool = false
            member this.modelUpdated(): unit = ()
            member this.preRollback(context: UpdateContext): unit = ()
            member this.postRollback(context: UpdateContext): unit = ()

        interface Org.Kevoree.Core.Api.NodeType with
            member this.plan(actualModel: IContainerRootMarshalled, targetModel:IContainerRootMarshalled, traces:ITracesSequence): AdaptationModel =
                plan actualModel targetModel (this.modelService.getNodeName()) traces

                (*
                    Replacer tous les cast par des demandes explicites de cast dans le context initial (donc dans le marshalled)
                *)
            member this.getPrimitive(primitive: AdaptationPrimitive): Org.Kevoree.Core.Api.Command.ICommand =
                let nodeName = this.modelService.getNodeName()
                match primitive.getType() with
                | Org.Kevoree.Core.Api.AdaptationType.AddDeployUnit ->
                    let deployUnit = primitive.getRef().CastToDeployUnit()
                    new AddDeployUnitCommand(deployUnit, this.bootstrapService, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.RemoveDeployUnit ->
                    let deployUnit = primitive.getRef().CastToDeployUnit()
                    new RemoveDeployUnitCommand(deployUnit, this.bootstrapService, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.UpdateInstance -> new NullCommand() :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.UpdateDictionary -> 
                    let inst = primitive.getRef().CastToInstance()
                    let value = primitive.getRef2().CastToValue()
                    new UpdateDictionaryCommand(inst, value, nodeName, this.modelRegistry, this.bootstrapService, this.modelService, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.AddInstance ->
                    let inst = primitive.getRef().CastToInstance()
                    new AddInstanceCommand(inst, nodeName, this.modelRegistry, this.bootstrapService, this.modelService, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.RemoveInstance ->
                    let inst = primitive.getRef().CastToInstance()
                    new RemoveInstanceCommand(inst, nodeName, this.modelRegistry, this.bootstrapService, this.modelService, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.AddBinding ->
                    let bind = primitive.getRef().CastToMBinding()
                    new AddBindingCommand(bind, nodeName, this.modelRegistry, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.RemoveBinding -> 
                    let bind = primitive.getRef().CastToMBinding()
                    new RemoveBindingCommand(bind, nodeName, this.modelRegistry, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.StartInstance ->
                    let inst = primitive.getRef().CastToInstance()
                    new StartStopInstanceCommand(inst, nodeName, true, this.modelRegistry, this.bootstrapService, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand
                | Org.Kevoree.Core.Api.AdaptationType.StopInstance ->
                    let inst = primitive.getRef().CastToInstance()
                    new StartStopInstanceCommand(inst, nodeName, false, this.modelRegistry, this.bootstrapService, this.logger) :> Org.Kevoree.Core.Api.Command.ICommand

            member this.Start():unit = ()
        inherit System.MarshalByRefObject        
    end
    
type DotnetNode with
    new() = {
        modelService = null
        bootstrapService = null
        context = null
        logLevel = "INFO"
        logger = null
        modelRegistry = new ModelRegistry()
    }