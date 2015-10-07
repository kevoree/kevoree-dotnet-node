namespace Org.Kevoree.Library

module AdaptationType =

    ///[<System.FlagsAttribute>]
    (*type AdaptationTypeFS =
        | AddDeployUnit = 1
        | RemoveDeployUnit = 2
        | UpdateInstance =3
        | UpdateBinding = 4
        | UpdateDictionaryInstance =5
        | AddInstance = 6
        | RemoveInstance = 7
        | AddBinding = 8
        | RemoveBinding = 9
        | StartInstance = 10
        | StopInstance = 11
        | LinkDeployUnit = 12
        | UpdateCallMethod = 13
        | UpgradeInstance = 14*)

    [<StructuredFormatDisplay("Adaptation({Type}, {PrimitiveType}, {Ref})"); CustomEquality; CustomComparison>]
    type AdaptationFS  =
        {   Type: Org.Kevoree.Core.Api.AdaptationType;
            NodePath: string;
            Ref: Org.Kevoree.Core.Api.IMarshalled.IKMFContainerMarshalled;
            Ref2: Org.Kevoree.Core.Api.IMarshalled.IKMFContainerMarshalled option }
        override this.Equals(yobj) =
            match yobj with
                | :? AdaptationFS as y  -> y.Type = this.Type && y.Ref = this.Ref && y.Ref2 = this.Ref2
                | _ -> false
        override this.GetHashCode() = hash (this.Type.GetHashCode(), this.NodePath.GetHashCode(), this.Ref.GetHashCode(), this.Ref2.GetHashCode())
        interface System.IComparable with
            member this.CompareTo  yobj =
                match yobj with
                | :? AdaptationFS as y -> compare (this.Type, this.NodePath) (y.Type, y.NodePath)
                | _ -> invalidArg "yobj" "cannot compare values of different types"


    type AdaptationModelFS = Set<AdaptationFS>

    type ModelRegistry = Map<string,obj>