import { useState } from "react";
import GameEmbed from "@/components/GameEmbed";
import HighScoresTable from "@/components/HighScoresTable";
import "bootstrap-icons/font/bootstrap-icons.css"; // Import Bootstrap Icons CSS

export default function Home() {
  const [isPanelVisible, setIsPanelVisible] = useState(false);

  const togglePanel = () => {
    setIsPanelVisible(!isPanelVisible);
  };

  return (
    <div className="flex h-screen w-screen bg-gray-100 overflow-hidden relative">
      {/* Game Section */}
      <div
        className={`transition-all duration-300 ${
          isPanelVisible ? "w-3/5" : "w-full"
        } bg-black shadow-lg m-4 rounded-lg overflow-hidden`}
      >
        <GameEmbed />
      </div>

      {/* Toggle Icon */}
      <div
        className="fixed top-10 transform -translate-y-1/2 right-0 bg-blue-500 text-white p-3 rounded-l-lg cursor-pointer z-50"
        onClick={togglePanel}
      >
        <i className="bi bi-trophy text-2xl"></i>
      </div>

      {/* High Scores Panel */}
      <div
        className={`fixed top-0 right-0 h-full bg-white shadow-lg transition-transform transform ${
          isPanelVisible ? "translate-x-0" : "translate-x-full"
        } w-2/5 p-6 overflow-y-auto z-40`}
      >
        <h2 className="text-xl font-semibold mb-4">High Scores</h2>
        <HighScoresTable />
      </div>
    </div>
  );
}
