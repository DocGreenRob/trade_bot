using System.Collections.Generic;

public class ProfitLoss{
    public double PercentOpen {get; set;}
    public double Open {get; set;}
    public double OpenDay {get; set;}
}

public interface IPosition{
    // dsggsd
}

public class Stock : IPosition{
    public 
}

public class Option : IPosition{
    
}

public enum OptionType{
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

public class Position{
    public int PositionId {get; set;}
    public double UnderlyingPriceAtEntry {get; set;}
    public IPosition PositionType {get; set;}
    public DateTime EntryTime {get; set;}
    public double ProfitLossOpen
}

public class Asset{
    public List<Position> Positions {get; set;}
}

public void Main(){
    Asset asset = new Asset();
    asset.Positions = new List<Position>();
    asset.Positions.Add(new Position{
        PositionId = 1,
        UnderlyingPriceAtEntry = 200,
        PositionType = Option
    });
}