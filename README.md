# GitChangeWatcher

**GitChangeWatcher**는 Windows 환경에서 특정 Git 저장소의 파일 변경 사항을 실시간으로 감지하고 자동으로 커밋 메시지를 생성하며 Git 명령어(`add`, `commit`, `push`)를 실행할 수 있도록 돕는 Windows Forms 기반 응용 프로그램입니다.

## 주요 기능

1. **파일 변경 감지**
   - `.cs`, `.c`, `.cpp`, `.java`, `.py` 파일의 생성, 수정, 이름 변경을 실시간으로 감지
   - 변경 사항 발생 시 사용자에게 알림 및 커밋 옵션 제공

2. **자동 커밋 메시지 생성**
   - 파일명이 숫자인 경우 백준(BOJ) 문제 제목을 가져와 커밋 메시지에 포함
   - 파일 확장자에 따른 메시지 접두사 제공 (예: `[C언어]`, `[Java]` 등)

3. **트레이 아이콘 지원**
   - 작업 표시줄의 시스템 트레이에서 실행 상태 확인 및 제어 가능
   - 최근 커밋 로그 확인, 감시 디렉토리 변경, 프로그램 종료 기능 제공

4. **로그 기록**
   - 커밋 내역을 `%AppData%\GitChangeWatcher\commits_{repoName}.log`에 저장

## 요구 사항

- **프레임워크**: .NET 8.0 (Windows Forms)
- **대상 플랫폼**: Windows 10 이상
- **필수 조건**: Git 클라이언트 설치 및 환경 변수에 등록

## 동작 방식

1. 프로그램 실행 시 Git 저장소 루트를 선택해야 하며 선택한 경로는 `%AppData%\GitChangeWatcher\repoPath.txt`에 저장됩니다.
2. 백그라운드에서 동작하며 시스템 트레이에 아이콘이 등록됩니다.
3. 트레이 아이콘을 우클릭하여 메뉴를 통해 프로그램을 제어할 수 있습니다.

- 프로그램 사용법은 [사용법 문서](instruction.md)에 자세히 안내되어 있습니다.

## 구조 및 주요 클래스

프로그램은 다음 주요 구성 요소로 이루어져 있습니다:

### 1. `MainForm`
- 파일 변경 사항을 감지하고 사용자에게 알림을 제공하는 **UI 폼**.
- **FileSystemWatcher**를 사용하여 Git 저장소의 파일 변경 이벤트를 처리.
- 변경된 파일에 대해 사용자 동작(`확인` 또는 `취소`)을 수집.
- Git 명령어 실행은 `GitHelper` 클래스를 호출하여 처리.

### 2. `CommitLogForm`
- 최근 커밋 로그를 표시하는 **UI 폼**.
- 로그 파일(`commits_{repoName}.log`)에서 데이터를 읽어와 사용자에게 표시.

### 3. `GitHelper`
- **Git 명령어 실행**과 관련된 헬퍼 클래스.
  - `add`, `commit`, `push` 명령을 실행.
  - 파일명을 기반으로 BOJ 문제 제목을 가져와 커밋 메시지 생성.
  - Git 명령 실행 실패 시 예외를 던져 오류를 처리할 수 있도록 지원.

### 4. `CommitLogManager`
- 커밋 로그 파일의 생성 및 관리.
  - 로그 파일에 새로운 기록 추가.
  - 기존 로그 파일을 읽어와 배열로 반환.

### 5. `UserDataPath`
- **사용자 데이터 경로 관리**를 담당.
  - `%AppData%\GitChangeWatcher` 폴더 생성 및 관리.
  - 저장소 경로(`repoPath.txt`) 및 커밋 로그 파일(`commits_{repoName}.log`) 경로 제공.

## 코드 예시

### Git 명령어 실행 (`GitHelper` 클래스)
```csharp
public static void ExecuteGitCommands(string workingDirectory, string fileName, string commitMessage)
{
    RunGitCommand(workingDirectory, $"add \"{fileName}\"");
    RunGitCommand(workingDirectory, $"commit -m \"{commitMessage}\"");
    RunGitCommand(workingDirectory, "push");
}
```

### 커밋 메시지 생성 (`GitHelper` 클래스)
```csharp
public static string GenerateCommitMessage(string filePath)
{
    string extension = Path.GetExtension(filePath).ToLower();
    string prefix = extension switch
    {
        ".cs" => "[C#]",
        ".c" => "[C언어]",
        ".cpp" => "[C++]",
        ".java" => "[Java]",
        ".py" => "[Python]",
        _ => "[Unknown]"
    };

    string problemTitle = FetchProblemTitle(filePath) ?? "알 수 없는 문제";
    return $"{prefix} {problemTitle}";
}
```

## 파일 및 로그 관리

- **로그 파일**: `%AppData%\GitChangeWatcher\commits_{repoName}.log`
  - 커밋 로그 형식: `[날짜 시간] | CommitMsg: [커밋 메시지] | FilePath: [파일 경로]`
- **저장소 경로 파일**: `%AppData%\GitChangeWatcher\repoPath.txt`
  - 감시 중인 Git 저장소의 루트 경로 저장

## 기여 방법

`GitChangeWatcher`에 기여하려면 다음 단계를 따르세요:

1. **저장소 포크**  
    - 자신의 GitHub 계정에 저장소를 포크합니다.

2. **브랜치 생성**  
    - 새로운 브랜치를 생성하세요.
        ```bash
        git checkout -b feature/your-feature-name
        ```

3. **코드 수정 및 테스트**  
   - 프로젝트에 기여할 기능을 구현하거나 버그를 수정하세요.
   - 수정한 코드가 기존 기능과 충돌하지 않는지 확인하세요.

4. **커밋 및 푸시**  
   - 수정한 내용이 잘 드러나는 커밋 메시지를 작성하고 푸쉬합니다.
        ```bash
        git commit -m "설명"
        git push origin feature/your-feature-name
        ```

5. **풀 리퀘스트 제출**  
    - GitHub 저장소에 Pull Request를 생성하여 변경 사항을 설명합니다.

## 문의 및 지원

프로그램 사용 중 문제가 발생하거나 개선 사항을 제안하고 싶다면 다음 방법으로 문의해 주세요:

1. **GitHub Issues**:  
   저장소의 Issues 섹션을 통해 버그 리포트 또는 기능 요청을 작성해 주세요.

2. **이메일 문의**:  
   추가적인 지원이 필요할 경우 아래 이메일 주소로 문의해 주세요: [potterLim0808@gmail.com](mailto:potterLim0808@gmail.com)