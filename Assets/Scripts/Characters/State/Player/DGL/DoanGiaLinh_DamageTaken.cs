using System.Collections.Generic;
using System.Linq;

public class DoanGiaLinh_DamageTaken : PlayerDamageTakenState
{
    public DoanGiaLinh_DamageTaken(Character character) : base(character)
    {
    }
    
    protected override void SetDamageTakenFinished()
    {
        // Giữ nguyên các logic khác nếu cần
        base.SetDamageTakenFinished();
    }

    // Xóa toàn bộ các phương thức cũ liên quan đến vũng máu độc
}