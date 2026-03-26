## 싱글톤

인터페이스 상속이 필요한 클래스에서 `ClassName.Method()` 형태의 직관적인 정적 호출 인터페이스를 제공하기 위해, 내부 인스턴스에 로직을 위임하는 구조를 정의합니다. 
인터페이스 상속을 유지하면서도 싱글톤 객체 접근 시 `ClassName.Method()` 형식으로 즉시 호출할 수 있는 도구적 편의성을 확보하는 것이 주 목적입니다.

### 코드 예시

```csharp
public interface ISystemService
{
    void ExecuteService();
}

public partial class SystemManager : ISystemService
{
    // private static 필드 instance에 객체 참조 저장
    private static SystemManager instance = null!;

    // static Initialize 함수를 통해 인스턴스 생성 및 초기화
    public static void Initialize()
    {
        if (instance != null) return;
        instance = new SystemManager();
    }

    // 생성자를 private으로 선언하여 외부 생성을 방지
    private SystemManager() { }

    // instance.Internal() 형식으로 멤버 함수를 호출하는 static 함수
    public static void Execute() => instance.InternalExecute();

    // 접근 제한자 internal과 Internal 접두사를 사용한 멤버 함수
    internal void InternalExecute()
    {
        // 실제 로직 구현
        ExecuteService();
    }

    public void ExecuteService()
    {
        // 인터페이스 구현
    }
}
```

---

### 목적

* **직관적인 호출 구조**: 싱글톤 객체 접근 시 `Instance` 프로퍼티를 매번 거치지 않고, `ClassName.Method()` 형식으로 즉시 호출할 수 있는 편의성을 제공합니다.
* **인터페이스 호환성**: C#의 `static class`가 가질 수 없는 인터페이스 상속 및 다형성 기능을 일반 클래스 구조에서 확보합니다.

### 싱글톤 구성 규칙

* **생성자 제한**: 클래스 외부에서 인스턴스를 임의로 생성할 수 없도록 반드시 `private` 생성자를 선언해야 합니다.
* **정적 초기화**: 인스턴스 생성은 반드시 `static void Initialize()` 메서드 내에서 수행하며, 시스템 기동 시점에 호출되어 필요한 의존성을 주입받아야 합니다.
* **인스턴스 필드**: 싱글톤 객체를 참조하는 필드는 `private static T instance` 형식을 사용합니다.

### 함수 명명 및 외부 노출 규칙

외부로 공개되는 모든 기능은 정적 함수와 멤버 함수의 쌍(Pair)으로 구성하여 호출 구조를 분리합니다.

1.  **공개용 정적 함수 (Public Static)**:
    * 외부에서 클래스명으로 즉시 접근하는 진입점입니다.
    * 접두사 없이 기능을 명확히 나타내는 이름을 사용합니다.
    * 내부 구현 없이 `instance.InternalMethod()` 형식으로 대응되는 멤버 함수를 호출합니다.

2.  **로직용 멤버 함수 (Internal Member)**:
    * 실제 데이터를 조작하거나 인터페이스 규약을 충족하는 본체입니다.
    * 접근 제한자는 `internal`을 사용하며, 함수명 앞에 `Internal` 접두사를 붙입니다.
    * 함수 내부에서는 `instance` 참조 없이 클래스의 필드와 속성에 직접 접근하여 코드를 작성합니다.
