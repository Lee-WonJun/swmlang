module AST

type VarName = string
type FuncName = string

type Call = {
    Name: FuncName
    Args: Map<VarName, int64>
}

type Statement =
    | Declare of VarName * int64
    | Increment of VarName
    | Decrement of VarName
    | Assign of VarName * int64
    | PutChar of VarName
    | PrintInt of VarName
    | While of VarName * Block
    | If of VarName * thenBlock: Block * elseBlock: Block option
    | CallIgnore of Call
    | CallAssign of VarName * Call
    | Return of VarName
and Block = Statement list

type FunctionDef = {
    Name: FuncName
    Parameters: (VarName * int64) list
    Body: Block
}

type Program = {
    Functions: Map<FuncName, FunctionDef>
    TopLevel: Block
}
