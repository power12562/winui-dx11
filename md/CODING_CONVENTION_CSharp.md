# 요약
```csharp
namespace WsiuEditor.Example
{
    // 인터페이스: I + PascalCase
    internal interface IExample
    {
        void DoWork();
    }

    // 클래스: PascalCase
    // 기능이 복잡할 경우 partial을 사용하여 파일 분리 (ExampleEditor.Main.cs, ExampleEditor.Internal.cs 등)
    internal partial class ExampleEditor : ImguiBase
    {
        // 비공개 정적 필드: camelCase (언더바 없음)
        private static int globalCounter = 0;

        // 비공개 인스턴스 필드: _camelCase
        private int _myId;
        private bool _isActive;

        // 공개 프로퍼티/필드: PascalCase
        public string Title { get; set; }

        public ExampleEditor(Engine engine, UInt64 id) : base(engine, id)
        {
            // 정적 멤버 접근: 항상 ClassName.Member 형태 유지
            _myId = ExampleEditor.globalCounter;
            ExampleEditor.globalCounter++;
        }

        public void Update(int targetId) // 매개변수: camelCase
        {
            int localValue = 10; // 지역 변수: camelCase
            
            // 디버깅 친화: Allman Style, 리터럴은 우측, 한 줄 if문 금지
            if (targetId == 100) 
            {
                _myId = localValue;
            }

            if (_engine == null)
            {
                return;
            }
        }
    }
}
```

# 1. 명명 규칙

| 분류 | 규칙 | 예시 | 비고 |
| :--- | :--- | :--- | :--- |
| **클래스** | PascalCase | EditorManager | |
| **인터페이스** | **I** + PascalCase | IEditor | |
| **비공개 멤버** | **_** + camelCase | _activeEditors | 객체 내부 상태 |
| **비공개 정적** | camelCase | editorRegistry | 클래스 공유 상태 |
| **공개 멤버** | PascalCase | IsVisible | 프로퍼티 및 필드 공통 |
| **지역 변수** | camelCase | currentIndex | 매개변수 포함 |
| **불리언 변수** | **is, can, was** 등 | _isActive | 접두사 필수 |

---

# 2. 코드 스타일

### 중괄호 및 줄 바꿈
- 모든 중괄호(`{ }`)는 반드시 **새 줄**에서 시작합니다 (Allman Style).
- `if`, `for`, `while` 문 등은 실행 문장이 하나더라도 **절대로 한 줄로 작성하지 않습니다.** 이는 디버깅 시 브레이크포인트를 명확히 설정하기 위함입니다.

```csharp
// 올바른 예시
if (condition == true)
{
    DoSomething();
}

// 잘못된 예시
if (condition == true) DoSomething();
```

### 비교 연산
- 비교 시 변수를 좌측에, 리터럴(`null`, `true`, `0` 등)을 **우측**에 배치합니다.
- `bool` 비교 시 생략하지 않고 `== true` 또는 `== false`를 명시하여 논리를 명확히 합니다.

```csharp
// 올바른 예시
if (item == null)
{
    return;
}

if (_isOpened == false)
{
    CloseInternal();
}
```

### 정적 멤버 접근
- 클래스 내부에서 `static` 멤버에 접근할 때도 가독성을 위해 반드시 **`ClassName.Member`** 형태로 호출합니다.

```csharp
// 올바른 예시
EditorManager.editorRegistry.Add(type, factory);
```

---

# 3. 작성 요령

### partial 클래스 적극 활용
- 클래스의 규모가 커지거나 기능별로 논리적 분리가 필요한 경우 `partial` 키워드를 사용하여 파일을 분리합니다.
- 파일명은 `클래스명.기능명.cs` 형식을 권장합니다.
    - 예: `EditorManager.Registration.cs` (타입 수집 및 등록 로직)
    - 예: `EditorManager.Render.cs` (ImGui 렌더링 로직)
- 이를 통해 단일 파일의 코드 비대화를 방지하고 유지보수성을 높입니다.

### 이름 중복 방지
- 매개변수와 지역변수 이름이 겹칠 경우 별도의 접두사를 붙이기보다 `newId`, `targetId` 등 의미 있는 이름을 사용하여 구분합니다.

### 디버깅 가독성
- 연속된 멤버 액세스 연산자(`.`) 호출보다는 중간 단계를 지역 변수에 할당하여 조사식(Watch)에서 확인하기 쉽게 작성합니다.

