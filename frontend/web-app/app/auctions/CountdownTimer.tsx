'use client';
import { useBidStore } from "@/hooks/useBidStore";
import { usePathname } from "next/navigation";
import Countdown from "react-countdown";

const render = ({days, hours, minutes, seconds, completed}: { days: number, hours: number; minutes: number; seconds: number; completed: boolean; }) => {
    
    return (
        <div className={`
            border-2 border-white text-white py-1 px-2 rounded-lg flex justify-center
        ${completed ? 'bg-red-600' : (days === 0 && hours < 10 ) ? 'bg-amber-600' : 'bg-green-600'} 
        
        `}>
            {completed ? (
            <span>Finished!</span> 
            ) : (
            <span>{days > 0 ? `${days}d ` : ''}{hours.toString().padStart(2, '0')}:{minutes.toString().padStart(2, '0')}:{seconds.toString().padStart(2, '0')}</span>
            )}
        </div>
    )
    
    if (completed) {
        // Render a completed state
        return <span>Finished!</span>;
    } else {
        // Render a countdown
        return <span>{hours}:{minutes}:{seconds}</span>;
    }
}

type Props = {
    auctionEnd: string;
};


export default function CountdownTimer({ auctionEnd }: Props) {

    const setOpen = useBidStore((state) => state.setOpen);
    const pathname = usePathname();

    function auctionFinished() {
        if (pathname.startsWith('/auctions/details')) {
            setOpen(false);
        }
    }
  
  return (
    <div>
      <Countdown date={auctionEnd} renderer={render} onComplete={auctionFinished} />
    </div>
  );
}
