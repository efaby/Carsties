import { getDetailsViewData } from "@/app/actions/auctionAction";
import { getCurrentUser } from "@/app/actions/authAction";
import Heading from "@/app/components/Heading";
import AuctionForm from "../../AuctionForm";

export default async function Update({ params }: { params: { id: string } }) {
  const { id } = await params;
  const data = await getDetailsViewData(id);
  const user = await getCurrentUser();
  return (
    <div className="mx-auto max-w-[75%] shadow-lg p-10 bg-white rounded-lg">
      <Heading title="Update your Auction" subtitle="Please update the details of your car (only these auction properties can be updated)"/>
      <AuctionForm auction={data} />
    </div>
  );
}
