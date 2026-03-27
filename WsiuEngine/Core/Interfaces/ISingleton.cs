namespace WsiuEngine.Core.Interfaces
{
    /// <summary>
    /// Engine 클래스의 싱글톤 인스턴스 추적용 인터페이스입니다. <br/>
    /// 클래스 외부에서는 [ClassName.Funtion()] 형태의 정적 호출을 원칙으로 하며, <br/>
    /// 본 인터페이스는 Engine에 인스턴스 전달이 필요한 특수 상황에서 제한적으로 사용합니다. <br/>
    /// </summary>
    public interface ISingleton
    { }
}
