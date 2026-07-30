// SPDX-License-Identifier: MIT
pragma solidity 0.8.30;

import {ERC1155} from "@openzeppelin/contracts/token/ERC1155/ERC1155.sol";
import {Ownable} from "@openzeppelin/contracts/access/Ownable.sol";

/// @notice Optional collectible ownership. This contract never grants match powers.
contract GameItems is ERC1155, Ownable {
    constructor(string memory baseUri, address initialOwner)
        ERC1155(baseUri)
        Ownable(initialOwner)
    {}

    function setBaseUri(string calldata nextBaseUri) external onlyOwner {
        _setURI(nextBaseUri);
    }

    function mint(address account, uint256 id, uint256 amount, bytes calldata data)
        external
        onlyOwner
    {
        _mint(account, id, amount, data);
    }

    function mintBatch(
        address account,
        uint256[] calldata ids,
        uint256[] calldata amounts,
        bytes calldata data
    ) external onlyOwner {
        _mintBatch(account, ids, amounts, data);
    }
}
