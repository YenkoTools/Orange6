import { useEffect, useState } from "react";

interface VersionInfo {
  GitTag: string;
  BuildNumber: string;
  BuildDate: string;
  CommitHash: string;
}

export default function AppVersion() {
  const [version, setVersion] = useState<string | null>(null);
  const [buildDate, setBuildDate] = useState<string | null>(null);

  useEffect(() => {
    fetch("/api/version")
      .then((res) => res.json())
      .then((data: VersionInfo) => {
        setVersion(data.GitTag ?? data.BuildNumber);
        if (data.BuildDate) {
          setBuildDate(new Date(data.BuildDate).toLocaleDateString());
        }
      })
      .catch(() => {});
  }, []);

  if (!version) return null;

  return (
    <span className="text-xs text-gray-400 font-mono">
      {version}{buildDate && ` · ${buildDate}`}
    </span>
  );
}
