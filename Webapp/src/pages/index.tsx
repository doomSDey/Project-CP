import GameEmbed from "@/components/GameEmbed";
import HighScoresTable from "@/components/HighScoresTable";

export default function Home() {
    return (
        <div className="flex flex-col lg:flex-row h-screen w-screen bg-gray-100 overflow-hidden">
            {/* Game Section */}
            <div className="lg:w-3/5 w-full h-2/5 lg:h-full bg-black shadow-lg m-4 rounded-lg overflow-hidden">
                <GameEmbed />
            </div>

            {/* High Scores Section */}
            <div className="lg:w-2/5 w-full h-auto bg-white shadow-lg rounded-lg m-4 p-6 overflow-y-auto">
                <HighScoresTable />
            </div>
        </div>
    );
}
