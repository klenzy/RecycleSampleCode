import React, { Component } from "react";
import logger from "sabio-debug";
import * as fileService from "../../services/fileService";
import Upload from "./Upload";
import FileCard from "./FileCard";
import Pagination from "rc-pagination";
import "rc-pagination/assets/index.css";
import Search from "./Search";
import Preview from "./Preview";
import PropTypes from "prop-types";
import swal from "sweetalert";

const _logger = logger.extend("testForm");

class FileManager extends Component {
  state = {
    fileData: [{}],
    preloadFiles: [],
    id: null,
    mappedPreview: [],

    isLoadingImage: false,
    isViewing: false,
    isUploading: false,
    buttonType: "added",

    files: [],
    mappedFiles: [],
    pageIndex: 1,
    pageSize: 12,
    totalCount: 0,
    totalPages: 0
  };

  componentDidMount() {
    this.getAll();
  }

  getAll = () => {
    fileService
      .getPaginate(this.state.pageIndex - 1, this.state.pageSize)
      .then(
        this.setState(prevState => {
          return {
            ...prevState,
            isLoadingImage: true
          };
        })
      )
      .then(this.onGetFilesSuccess)
      .catch(this.onActionError);
  };

  onGetFilesSuccess = response => {
    const files = response.item.pagedItems;

    this.setState(prevState => {
      return {
        ...prevState,
        files,
        mappedFiles: files.map(this.mapFile),
        pageSize: response.item.pageSize,
        totalCount: response.item.totalCount,
        totalPages: response.item.totalPages,
        isLoadingImage: false
      };
    });
  };

  mapFile = file => (
    <FileCard
      fileData={this.state.file}
      file={file}
      key={file.id}
      view={this.onView}
    />
  );

  mapPreview = prev => <Preview prev={prev} />;

  onView = file => {
    this.setState(prevState => {
      return {
        ...prevState,
        fileData: [
          {
            url: file.url,
            id: file.id,
            entityTypeId: 6,
            name: file.name,
            ...prevState.fileData
          }
        ],
        id: file.id,

        isViewing: true,
        isUploading: false,
        buttonType: "updated"
      };
    });
  };

  newUpload = () => {
    this.setState(prevState => {
      return {
        ...prevState,
        fileData: [{}],
        isViewing: false,
        isUploading: false
      };
    });
  };

  handleConfirm = () => {
    this.state.isViewing
      ? fileService
          .update(this.state.fileData[0])
          .then(this.onAddSuccess)
          .then(this.newUpload)
          .catch(this.onUpdateError)
      : fileService
          .add(this.state.fileData)
          .then(this.onAddSuccess)
          .then(this.newUpload)
          .catch(this.onAddError);
  };

  handleAbort = () => {
    this.state.fileData.splice(0, this.state.fileData.length);
    this.newUpload();
  };

  handleImageChange = e => {
    const files = e.target.files;
    const formData = new FormData();
    let preloadFiles = [];
    if (files === null) {
      return;
    }
    for (let i = 0; i < files.length; i++) {
      formData.append("files", files[i]);
      preloadFiles.push({
        name: formData.getAll("files")[i].name,
        id: this.state.id
      });
    }
    this.setState(() => {
      return {
        preloadFiles,
        isLoadingImage: true
      };
    });
    fileService
      .upload(formData)
      .then(this.onUploadSuccess)
      .catch(this.onUploadError);
    _logger(formData.getAll("files"));
  };

  onUploadSuccess = response => {
    if (response.items === null) {
      return;
    }
    this.showPreview(response);
    if (!this.state.isViewing) {
      this.state.fileData.shift();
    }
    this.setState(prevState => {
      return {
        ...prevState,
        isLoadingImage: false
      };
    });

    _logger(this.state.fileData);
  };

  onUploadError = () => {
    this.handleAbort();
  };

  showPreview = response => {
    if (response === null) {
      return;
    }
    let fileData = [];
    for (let i = 0; i < this.state.preloadFiles.length; i++) {
      var obj = {
        url: response.items[i].url,
        entityTypeId: 6,
        name: response.items[i].fileName,
        fileTypeId: response.items[i].fileTypeId,
        id: this.state.id
      };
      fileData.push(obj);
    }

    this.state.isViewing
      ? this.setState(() => {
          return {
            fileData,
            isUploading: false,
            isLoadingImage: true,
            buttonType: "updated"
          };
        })
      : this.setState(prevState => {
          let prevFileData = [...prevState.fileData];
          return {
            ...prevState,
            fileData: prevFileData.concat(fileData),
            mappedPreview: response.items.map(this.mapPreview),
            isUploading: true,
            isViewing: false,
            isLoadingImage: true,
            buttonType: "added"
          };
        });
  };

  handleSearch = query => {
    fileService.search(query, this.state.pageSize).then(res => {
      this.onGetFilesSuccess(res);
    });
  };

  handlePage = pageNumber => {
    fileService
      .getPaginate(pageNumber, 6)
      .then(this.onActionSuccess)
      .catch(this.onActionError);
  };

  onChange = page => {
    this.setState({ pageIndex: page }, () => this.getAll());
  };

  onAddError = () => {
    swal({
      title: "Error!",
      text: `Could not be successfully ${this.state.buttonType}`,
      icon: "error"
    });
  };

  onUpdateError = () => {
    swal({
      title: "No Changes Made",
      text: `File was not ${this.state.buttonType}`,
      icon: "warning"
    });
  };

  onAddSuccess = () => {
    this.getAll();
    swal({
      title: `File has been successfully ${this.state.buttonType}!`,
      icon: "success"
    });
  };

  render() {
    return (
      <>
        <h1>File Manager</h1>
        <div className="container">
          <div className="row">
            <div className="col-md-4">
              <div>
                {" "}
                <Upload
                  onFileSelector={this.handleImageChange}
                  isViewing={this.state.isViewing}
                />
                {this.state.isViewing ? (
                  <span>Update File</span>
                ) : (
                  <span>Upload New File</span>
                )}
                {this.state.isLoadingImage ? (
                  <div className="mt-2">
                    <img
                      src="https://sabio-training.s3-us-west-2.amazonaws.com/Recycle-36025c92-1ddf-4811-9571-0deac0d464be_@re-cycle-signature-logo-320x320 (2).gif"
                      alt=""
                      height="70"
                      width="70"
                    />
                  </div>
                ) : this.state.isUploading ? (
                  <div className="mt-2">
                    <button
                      onClick={this.handleConfirm}
                      className="btn btn-success  float-right"
                    >
                      Save File(s)
                    </button>

                    <button
                      onClick={this.handleAbort}
                      className="btn btn-danger"
                    >
                      Abort Upload
                    </button>
                  </div>
                ) : null}
                {this.state.isViewing ? (
                  <div className="mt-2">
                    <button onClick={this.newUpload} className="btn btn-dark">
                      New File
                    </button>

                    <button
                      onClick={this.handleConfirm}
                      className="btn btn-success float-right"
                    >
                      Apply Update
                    </button>
                  </div>
                ) : null}
                {this.state.isUploading ? (
                  <div className="mt-2">{this.state.mappedPreview}</div>
                ) : null}
                {this.state.isViewing ? (
                  <>
                    <div className="mt-2">
                      {this.state.fileData[0]
                        ? this.state.fileData[0].name
                        : null}
                    </div>
                    <div className="mt-2">
                      <img
                        className="preview"
                        src={
                          this.state.fileData
                            ? this.state.fileData[0].url
                            : null
                        }
                        alt=""
                        height="200px"
                        width="338px"
                      />

                      <div className="center mt-3">
                        <h2 className="fa fa-download mr-2"> </h2>
                      </div>
                    </div>
                  </>
                ) : null}
              </div>
            </div>

            <div className="col-md-2" />

            <div
              className={`col-lg-6 ${
                this.state.isUploading ? "disabled" : null
              } right-side`}
            >
              <div className="search">
                <Search onSearching={this.handleSearch} />
              </div>

              <div>{this.state.mappedFiles}</div>
              <div className="mt-4">
                <Pagination
                  onChange={this.onChange}
                  current={this.state.pageIndex}
                  pageSize={this.state.pageSize}
                  total={this.state.totalCount}
                  showSizeChange
                  showTotal={(total, range) =>
                    `${range[0]} - ${range[1]} of ${total} items`
                  }
                />
              </div>
            </div>
          </div>
        </div>
      </>
    );
  }
}

FileManager.propTypes = {
  file: PropTypes.shape({
    url: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired
  })
};
export default FileManager;
