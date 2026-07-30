// SPDX-License-Identifier: MIT
pragma solidity 0.8.30;

import {GameItems} from "../src/GameItems.sol";

contract GameItemsTest {
    function testOwnerCanMintCollectibleWithoutGameplayMeaning() public {
        GameItems items = new GameItems("ipfs://collection/{id}.json", address(this));
        address player = address(0xBEEF);
        items.mint(player, 7, 2, "");
        assert(items.balanceOf(player, 7) == 2);
    }
}
