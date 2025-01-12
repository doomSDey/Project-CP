import GameEmbed from "@/components/GameEmbed";
import HighScoresTable from "@/components/HighScoresTable";

export default function Home() {
    return (
        <>
            <div className="flex flex-col items-center">
                <GameEmbed />
                <HighScoresTable />
            </div>
        </>
    );
}
