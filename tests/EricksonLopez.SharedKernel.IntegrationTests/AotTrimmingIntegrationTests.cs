// Copyright © Erickson Lopez. MIT License.
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EricksonLopez.SharedKernel.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Category", "AotTrimming")]
public class AotTrimmingIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public AotTrimmingIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void NativeAot_Publish_And_Execute_NativeAotTests_With_InvariantGlobalization_False_Succeeds()
    {
        var repoRoot = GetRepositoryRoot();
        var projectPath = Path.Combine(repoRoot, "tests", "EricksonLopez.SharedKernel.NativeAotTests", "EricksonLopez.SharedKernel.NativeAotTests.csproj");
        File.Exists(projectPath).Should().BeTrue($"Project file must exist at {projectPath}");

        var tempPublishDir = Path.Combine(repoRoot, ".temp", "SharedKernel_NativeAotTests_" + Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(Path.Combine(repoRoot, ".temp"));

            // 1. dotnet publish with PublishAot=true, InvariantGlobalization=false, TreatWarningsAsErrors=true, EnableTrimAnalyzer=true
            var publishArgs = $"publish \"{projectPath}\" --configuration Release -p:PublishAot=true -p:InvariantGlobalization=false -p:TreatWarningsAsErrors=true -p:EnableTrimAnalyzer=true -o \"{tempPublishDir}\"";
            _output.WriteLine($"Executing: dotnet {publishArgs}");

            var publishResult = RunDotNetCli(publishArgs, repoRoot);
            _output.WriteLine($"Publish StdOut:\n{publishResult.StdOut}");
            _output.WriteLine($"Publish StdErr:\n{publishResult.StdErr}");

            publishResult.ExitCode.Should().Be(0, because: $"dotnet publish with PublishAot=true and InvariantGlobalization=false must succeed. Errors: {publishResult.StdErr}");

            // Verify no IL trimming or AOT warnings were generated across combined output
            var combinedOutput = $"{publishResult.StdOut}\n{publishResult.StdErr}";
            combinedOutput.Should().NotContain("IL2026", because: "No trimmer IL2026 warnings should be produced.");
            combinedOutput.Should().NotContain("IL3050", because: "No AOT IL3050 warnings should be produced.");

            // 2. Locate native binary
            var exeExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;
            var binaryPath = Path.Combine(tempPublishDir, "EricksonLopez.SharedKernel.NativeAotTests" + exeExtension);

            File.Exists(binaryPath).Should().BeTrue($"Published native AOT binary must exist at {binaryPath}");

            // 3. Ensure executable permission on Linux / macOS
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                RunProcess("chmod", $"+x \"{binaryPath}\"", tempPublishDir);
            }

            // 4. Execute the published native binary directly
            var runResult = RunProcess(binaryPath, string.Empty, tempPublishDir);
            _output.WriteLine($"Binary StdOut:\n{runResult.StdOut}");
            _output.WriteLine($"Binary StdErr:\n{runResult.StdErr}");

            runResult.ExitCode.Should().Be(0, because: $"Native binary execution must return exit code 0. Stderr: {runResult.StdErr}");
            runResult.StdOut.Should().Contain("=== AOT Validator: OK ===", because: "All domain assertions in AOT binary must pass.");
        }
        finally
        {
            TryDeleteDirectory(tempPublishDir);
        }
    }

    [Fact]
    public void NativeAot_Publish_And_Execute_Sample_With_InvariantGlobalization_False_Succeeds()
    {
        var repoRoot = GetRepositoryRoot();
        var projectPath = Path.Combine(repoRoot, "samples", "EricksonLopez.SharedKernel.Sample", "EricksonLopez.SharedKernel.Sample.csproj");
        File.Exists(projectPath).Should().BeTrue($"Project file must exist at {projectPath}");

        var tempPublishDir = Path.Combine(repoRoot, ".temp", "SharedKernel_Sample_" + Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(Path.Combine(repoRoot, ".temp"));

            // 1. dotnet publish with PublishAot=true and InvariantGlobalization=false
            var publishArgs = $"publish \"{projectPath}\" --configuration Release -p:InvariantGlobalization=false -p:TreatWarningsAsErrors=true -p:EnableTrimAnalyzer=true -o \"{tempPublishDir}\"";
            _output.WriteLine($"Executing: dotnet {publishArgs}");

            var publishResult = RunDotNetCli(publishArgs, repoRoot);
            _output.WriteLine($"Publish StdOut:\n{publishResult.StdOut}");
            _output.WriteLine($"Publish StdErr:\n{publishResult.StdErr}");

            publishResult.ExitCode.Should().Be(0, because: $"dotnet publish on Sample must succeed. Errors: {publishResult.StdErr}");

            // Verify no IL trimming or AOT warnings were generated across combined output
            var combinedOutput = $"{publishResult.StdOut}\n{publishResult.StdErr}";
            combinedOutput.Should().NotContain("IL2026", because: "No trimmer IL2026 warnings should be produced.");
            combinedOutput.Should().NotContain("IL3050", because: "No AOT IL3050 warnings should be produced.");

            // 2. Locate native binary
            var exeExtension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;
            var binaryPath = Path.Combine(tempPublishDir, "EricksonLopez.SharedKernel.Sample" + exeExtension);

            File.Exists(binaryPath).Should().BeTrue($"Published native AOT binary must exist at {binaryPath}");

            // 3. Ensure executable permission on Linux / macOS
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                RunProcess("chmod", $"+x \"{binaryPath}\"", tempPublishDir);
            }

            // 4. Execute the published native binary directly
            var runResult = RunProcess(binaryPath, string.Empty, tempPublishDir);
            _output.WriteLine($"Binary StdOut:\n{runResult.StdOut}");
            _output.WriteLine($"Binary StdErr:\n{runResult.StdErr}");

            runResult.ExitCode.Should().Be(0, because: $"Native binary execution must return exit code 0. Stderr: {runResult.StdErr}");
            runResult.StdOut.Should().Contain("Showcase executed successfully", because: "All showcase levels must pass.");
        }
        finally
        {
            TryDeleteDirectory(tempPublishDir);
        }
    }

    private static string GetRepositoryRoot()
    {
        var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
        while (currentDir != null)
        {
            if (File.Exists(Path.Combine(currentDir.FullName, "EricksonLopez.SharedKernel.slnx")) ||
                File.Exists(Path.Combine(currentDir.FullName, "EricksonLopez.SharedKernel.sln")) ||
                File.Exists(Path.Combine(currentDir.FullName, "Directory.Build.props")))
            {
                return currentDir.FullName;
            }
            currentDir = currentDir.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root containing EricksonLopez.SharedKernel.slnx / .sln or Directory.Build.props.");
    }

    private static ProcessExecutionResult RunDotNetCli(string arguments, string workingDirectory)
    {
        return RunProcess("dotnet", arguments, workingDirectory);
    }

    private static ProcessExecutionResult RunProcess(string fileName, string arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.EnvironmentVariables["DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER"] = "1";
        startInfo.EnvironmentVariables["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.EnvironmentVariables["DOTNET_NOLOGO"] = "true";
        startInfo.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        startInfo.EnvironmentVariables["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";

        using var process = new Process { StartInfo = startInfo };
        var stdOutBuilder = new System.Text.StringBuilder();
        var stdErrBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                stdOutBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                stdErrBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var exited = process.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);
        if (!exited)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException($"Process '{fileName} {arguments}' timed out after 5 minutes.");
        }

        // Ensure async read buffers are flushed
        process.WaitForExit();

        return new ProcessExecutionResult(process.ExitCode, stdOutBuilder.ToString(), stdErrBuilder.ToString());
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup failures on temp dirs
        }
    }

    private record ProcessExecutionResult(int ExitCode, string StdOut, string StdErr);
}
