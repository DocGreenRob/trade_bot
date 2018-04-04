public class Position{
    public int PositionId {get; set;}
    public double UnderlyingPriceAtEntry {get; set;}
    public IPositionType PositionType {get; set;}
    public DateTime EntryTime {get; set;}
}

public interface IPositionType{
    // dsggsd
}

public class Stock : IPositionType{
    public 
}

public class Option : IPositionType{
    
}

public enum OptionPositionType{
    Naked,
    Stradle,
    Vertical
}

public enum ContractType{
    Call,
    Put
}

public void ManageProfit(){
    // sadfsdf
}

public bool IsReversal(int sessionInterval){
    // dfgsdg
}