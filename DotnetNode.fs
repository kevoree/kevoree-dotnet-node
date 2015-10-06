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

        [<Param(Optional = true, DefaultValue = "INFO")>] val mutable log:string

        member this.modelRegistry:ModelRegistry = new ModelRegistry();


        [<Start>]
        member this.Start() =
            this.modelService.registerModelListener(this)
            this.modelRegistry.registerByPath(this.context.getPath(), this)

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

            member this.getPrimitive(primitive: AdaptationPrimitive): PrimitiveCommand  = null

            member this.Start():unit = ()
        inherit System.MarshalByRefObject        
    end
    
type DotnetNode with
    new() = {
        modelService = null
        bootstrapService = null
        context = null
        log = "INFO"
    }