# MCP package and source provenance

The external net10.0 host consumes official `ModelContextProtocol` **2.2.0** and its locked dependencies. No MCP package enters the Revit/net48 process. No extra Microsoft.Extensions.Hosting package is needed: the host uses McpServer.Create and owns its streams/cancellation directly.

`src/mcp-server/CancellableInputStream.cs` adapts the private `CancellableStdinStream` idea from [StdioServerTransport.cs at 6fa3825973949a9c4f0cd8af344e15a8db09dc35](https://github.com/modelcontextprotocol/csharp-sdk/blob/6fa3825973949a9c4f0cd8af344e15a8db09dc35/src/ModelContextProtocol.Core/Server/StdioServerTransport.cs), official C# SDK v2.2.0. The modification retains a private buffer until a raw read finishes, observes abandoned faults and rejects reuse/concurrent reads. It cancels waiting without claiming to cancel Windows console I/O. Array and Memory overloads use the same path through the original lab WrappedStream. TraceStream and host composition are original lab code; the SDK implements all MCP dispatch/parsing.

The source repository's [full LICENSE](csharp-sdk-LICENSE.txt) records its Apache-2.0 transition and remaining MIT contributions; [THIRD-PARTY-NOTICES](csharp-sdk-THIRD-PARTY-NOTICES.txt) is retained unchanged. Preserve these notices when redistributing this adaptation. Package license expressions and the resolved graph are recorded separately in the dependency research.

The older **TypeScript SDK 1.26.0** and third-party Revit bridge remain learning precedents. Their code is not copied or installed here. The production MCP tools now reuse the existing lab BIM computation through the original local bridge; the separate compatibility probe remains synthetic and has no Revit access.

The Microsoft MIT license texts are retained as [runtime LICENSE](dotnet-runtime-LICENSE.txt) and [extensions LICENSE](dotnet-extensions-LICENSE.txt), fetched from the package-recorded source commits. [Source URLs and hashes](license-sources.json) and the [package evidence snapshot](../../../../docs/research/evidence/mcp-package-observations-2026-10-20.json) retain their provenance and package copyright fields. Package THIRD-PARTY-NOTICES files remain unchanged.

Nine Microsoft packages contain byte-identical runtime notices; [one unchanged copy](dotnet-runtime-THIRD-PARTY-NOTICES.txt) is retained with a [package-to-file/hash mapping](package-notices.json). The mapped bytes were checked against each package archive.

Publication check, October 26: the retained SDK LICENSE and THIRD-PARTY-NOTICES preserve upstream text using CRLF line endings; the pinned upstream files use LF. This is text equality after newline normalization, not byte equality with upstream. The retained files were left unchanged; exact digests are recorded in the [distribution review](../../../../docs/reference/public-distribution-review-2026-10-26.md).
