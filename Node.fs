namespace Org.Kevoree.Node

open Org.Kevoree.Annotation
open Org.Kevoree.Core.Api.Handler
open Org.Kevoree.Core.Api
open System

[<Org.Kevoree.Annotation.NodeType>]
type Node =
    class
        [<KevoreeInject>] val mutable modelService:ModelService

        [<KevoreeInject>] val mutable bootstrapService: BootstrapService

        [<KevoreeInject>] val mutable context: Context 

        [<Param(Optional = true, DefaultValue = "INFO")>] val mutable log:string

        interface DeployUnit
        interface ModelListener with
            member this.preUpdate(context: UpdateContext): bool = false
            member this.initUpdate(context: UpdateContext): bool = false
            member this.afterLocalUpdate(context: UpdateContext): bool = false
            member this.modelUpdated(): unit = ()
            member this.preRollback(context: UpdateContext): unit = ()
            member this.postRollback(context: UpdateContext): unit = ()
        inherit MarshalByRefObject        
    end
    
type Node with
    new() = {
        modelService = null
        bootstrapService = null
        context = null
        log = "INFO"
    }