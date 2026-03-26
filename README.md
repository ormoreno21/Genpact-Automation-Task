# Genpact Automation Assignment - Wikipedia Playwright

This repository contains a lightweight, robust UI and API test automation framework built for the Genpact automation assignment. The framework is designed using **Clean Architecture** principles and the **Page Object Model (POM)** design pattern.

## 🛠️ Technology Stack
* **Language:** C# (.NET)
* **Testing Framework:** NUnit
* **Browser Automation:** Selenium WebDriver
* **API Client:** `HttpClient` (for MediaWiki Parse API)
* **Reporting:** ExtentReports (Bonus Task)

## 🏗️ Architecture & Best Practices
* **Native Selenium Commands:** The framework strictly uses native Selenium WebDriver commands (e.g., `FindElements`, `GetCssValue`) for DOM traversal and UI interactions, avoiding excessive JavaScript injections.
* **Dynamic Waiting:** Implemented `WebDriverWait` for dynamic UI elements (like expanding tables and theme toggles).
* **Text Normalization:** Custom utility classes (`TextNormalization`, `WikiMarkupCleaner`) using Regex and `HashSet` to accurately normalize and count unique words across both UI and API responses.

## 🚀 How to Run the Tests

1. Ensure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed.
2. Clone the repository and navigate to the project root folder.
3. Open your terminal and run the following command:
   ```bash
   dotnet test

📊 Viewing the HTML Report (Bonus)
After the test execution finishes, a rich HTML report is automatically generated using ExtentReports.
You can find the index.html report file inside the compilation folder (e.g., bin/Debug/net8.0/Reports/ or similar, depending on your build configuration). Open it in any modern browser to view the detailed results.

⚠️ Important Note Regarding Task 2
Task 2 is expected to FAIL.
As per the assignment instructions: "validate that all the 'technology names' under this section are a text link, if not please fail the test".
During the test execution, the framework dynamically expands the "Microsoft development tools" Navbox. It identifies that the word "Playwright" is rendered as plain text (since it represents the current active article) and lacks an <a> tag. Consequently, the test explicitly catches this and fails the assertion, printing the exact non-link item to the report.