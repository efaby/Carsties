'use client';

import { useEffect, useState } from "react";
import { getData } from "../actions/auctionAction";
import AppPagination from "../components/AppPagination";
import AuctionCard from "./AuctionCard";
import { Auction, PageResult } from "../types";
import Filters from "./Filters";
import { useParamsStore } from "@/hooks/useParamsStore";
import { useShallow } from "zustand/shallow";
import queryString from "query-string";
import EmptyFilter from "../components/EmptyFilter";
import { useAuctionStore } from "@/hooks/useAuctionStore";


export default function Listings() {
  const [loading, setLoading] = useState(true);
  const params = useParamsStore(useShallow(state => ({
    pageNumber: state.pageNumber,
    pageSize: state.pageSize,
    searchTerm: state.searchTerm,
    orderBy: state.orderBy,
    filterBy: state.filterBy,
    seller: state.seller,
    winner: state.winner
  })));

  const data = useAuctionStore(useShallow(state => ({
    auctions: state.auctions,
    totalCount: state.totalCount,
    pageCount: state.pageCount
  })));


  const setData = useAuctionStore(state => state.setData);
  const setParams = useParamsStore(state => state.setParams);

  const url = queryString.stringifyUrl({
    url: '',
    query: params
  }, { skipEmptyString: true });

  function setPageNumber(page: number) {
    setParams({ pageNumber: page });
  }
  useEffect(() => {
    getData(url).then((data) => {
      setData(data);
      setLoading(false);
    });
  }, [url, setData]);

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <>
    <Filters />
    { data.totalCount === 0 ? (
      <EmptyFilter showReset />
    ) : (
      <>
        <div className="grid grid-cols-4 gap-6">
          {data && data.auctions.map((auction) => (
            <AuctionCard key={auction.id} auction={auction} />
          ))}
        </div>
        <div className="flex justify-center mt-4">
          <AppPagination pageChanged={setPageNumber} currentPage={params.pageNumber} pageCount={data.pageCount ? data.pageCount : 1} />
        </div>
      </>
    )}
    </>
    
  );
}
