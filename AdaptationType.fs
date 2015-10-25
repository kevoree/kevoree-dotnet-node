namespace Org.Kevoree.Library

module AdaptationType =
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

        override this.ToString() = 
            let ref2str = match this.Ref2 with
                | Some(x) -> x.path()
                | None -> ""
            "AdaptationTypeFS [Type = " + this.Type.ToString() + "; RefPath = " + this.Ref.path() + "; Ref2Path = " + ref2str + "]\n"
        interface System.IComparable with
            member this.CompareTo  yobj =
                match yobj with
                | :? AdaptationFS as y -> 
                    let thisCodeRef2 = match this.Ref2 with
                        | None -> ""
                        | Some(x) -> x.ToString()
                    let thatCodeRef2 = match y.Ref2 with
                        | None -> ""
                        | Some(x) -> x.ToString()
                    let cmp = compare (this.Type, this.Ref.ToString(), thisCodeRef2) (y.Type, y.Ref.ToString(), thatCodeRef2)
                    cmp
                | _ -> invalidArg "yobj" "cannot compare values of different types"


    type AdaptationModelFS = Set<AdaptationFS>

    type ModelRegistry = Map<string,obj>