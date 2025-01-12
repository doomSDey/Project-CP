import React from 'react';

const GameEmbed: React.FC = () => {
  return (
    <div className="flex justify-center items-center h-screen bg-black">
      <iframe
        src="https://project-capybara.s3.ap-south-1.amazonaws.com/index.html"
        title="Unity Game"
        className="w-full h-full"
        allowFullScreen
      ></iframe>
    </div>
  );
};

export default GameEmbed;
