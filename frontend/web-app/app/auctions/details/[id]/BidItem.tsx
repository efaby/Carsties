import { Bid } from "@/app/types";
import { numberWithCommas } from "@/lib/numberWithComma";
import { format } from "date-fns/fp/format";

type Props = {
    bid: Bid;
}

export default function BidItem({ bid }: Props) {
    function getBidInfo(bid: Bid) {
        let bgColor = '';
        let text = '';
        switch (bid.bidStatus) {
            case 'Accepted':
                bgColor = 'bg-green-200';
                text = 'Bid Accepted';
                break;
            case 'AcceptedBelowReserve':
                bgColor = 'bg-amber-500';
                text = 'Reserve not met';
                break;
            case 'TooLow':
                bgColor = 'bg-red-200'; 
                text = 'Bid was too low';
                break;

            default:
                bgColor = 'bg-gray-200';
                text = 'Bid placed after auction finished';
                break; 
        }
        return { bgColor, text }; 

    }
    return (
        <div className={`border-gray-300 border-2 px-3 py-2 rounded-lg flex justify-between items-center mb-2 ${getBidInfo(bid).bgColor}`}>
            <div className="flex flex-col">
                <span>Bidder {bid.bidder}</span>
                <span className="text-gray-700 text-sm">Time: {bid.bidTime}</span>
            </div>
            <div className="flex flex-col text-right">
                <div className="text-lg font-semibold">${numberWithCommas(bid.amount)}</div>
                <div className="flex flex-row items-center">
                    <span>{getBidInfo(bid).text}</span>
                </div>
            </div>
        </div>
    );
}
